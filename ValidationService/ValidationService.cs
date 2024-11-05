using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Domain.Queue;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using CommonServices.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Collections.Concurrent;


namespace ValidationService
{
    class Program
    {
      
        static void Main(string[] args)
        {
            const string AppName = "ValidationService";
            const string SqlConnectionString = "Server=localhost,1433;Database=Reservations;User Id=sa;Password=MyStrongS1rootP@ssword;TrustServerCertificate=true";
            const string ReceivingQueue = QueueNames.Validation.Receive;
            const string ReplyToQueue = QueueNames.Validation.Send;
           
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",   
                UserName = "user",     
                Password = "password"  
            };
            IRabbitMqConnectionFactory rabbitMqConnectionFactory = new RabbitMqConnectionFactory(connectionFactory);

            IMessageProcessor messageProcessor = new RabbitMqMessageProcessor(rabbitMqConnectionFactory, ReceivingQueue,ReplyToQueue);
            
           
            IFileLogger fileLogger = new FileLogger();         
            var messageHandler = new CustomMessageHandler(fileLogger, SqlConnectionString, AppName);

            // Create a cancellation token source to manage stopping the service
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Cancellation requested...");
                cancellationTokenSource.Cancel();
                e.Cancel = true; // Prevents immediate termination
            };


            // Start producer with a handler that enqueues messages
            Task producerTask = Task.Run(() => messageProcessor.StartProducer(CancellationToken.None, ((RabbitMqMessageProcessor)messageProcessor).EnqueueMessage));
         

            // Start consumers with the dynamic message handling logic
            Task consumerTask = Task.Run(() => messageProcessor.StartConsumers(CancellationToken.None, messageHandler.HandleMessageAsync));

            try
            {
                Console.WriteLine("Validation Service is running. Press Ctrl+C to stop.");
                Task.WaitAll(producerTask, consumerTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Service is stopping gracefully...");
            }
            finally
            {
                // Perform cleanup if necessary
                messageProcessor.StopProcessing();
                Console.WriteLine("Service has stopped.");
            }

            //=====================================================================================================================================================
            //private ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();

            //end of initialization here

            var model = new StoreResultToDb
            {
                Raw = "Sample raw data",
                Dt = DateTime.Now,
                ValidationResult = 9               
            };

            //idva msg... dobavia se v edna colecsia... prawi all this things... i nakraq se trie.

            //Create Log messages thread-safe FIFO collection
            var logMessagesCollection = new List<string>();



            /*
            //MSSQL Insert
            try
            {
                string sqlConnectionString = "Server=localhost,1433;Database=Reservations;User Id=sa;Password=MyStrongS1rootP@ssword;TrustServerCertificate=true";
                var dbManager = DatabaseManagerFactory.CreateDatabaseManager(DatabaseType.SqlServer, sqlConnectionString);
                await dbManager.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model);
                if (model.Result == 1)
                {
                    logMessagesCollection.Add("Request result inserted successfully");
                }
                else
                {
                    logMessagesCollection.Add(AppName + " " + model.ResultText);
                }
            }
            catch (Exception ex)
            {
                logMessagesCollection.Add(AppName + " " + ex.Message);
            };

            //POSTGRESQL Insert
            try
            {
                string pgConnectionString = "Host=localhost;Port=5432;Database=Reservations;Username=postgres;Password=YourStrong@Passw0rd";
                var dbManager = DatabaseManagerFactory.CreateDatabaseManager(DatabaseType.PostgreSql, pgConnectionString);
                await dbManager.ExecuteStoredProcedureAsync("sp_insertrequestresult", model);
                if (model.Result == 1)
                {
                    logMessagesCollection.Add("Request result inserted successfully");
                }
                else
                {
                    logMessagesCollection.Add(AppName + " " + model.ResultText);
                }
            }
            catch (Exception ex)
            {
                logMessagesCollection.Add(AppName + " " + ex.Message);
            };

            await logger?.LogAsync(logMessagesCollection);            
            */
        }       

    }
}