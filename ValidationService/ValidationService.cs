using CommonServices.Domain.Enums;
using CommonServices.Domain.Queue;
using CommonServices.Infrastructure.Logging;
using CommonServices.Infrastructure.Messaging;
using RabbitMQ.Client;


namespace ValidationService
{
    class Program
    {

        static void Main(string[] args)
        {
            const string AppName = "ValidationService";
            const string SqlConnectionString = "Server=localhost,1433;Database=Reservations;User Id=sa;Password=MyStrongS1rootP@ssword;TrustServerCertificate=true";
            const string Qname = QueueNames.Validation.Receive;            

            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",
                Password = "password"
            };

            IRabbitMqConnectionFactory rabbitMqConnectionFactory = new RabbitMqConnectionFactory(connectionFactory);

            IMessageProcessor messageProcessor = new RabbitMqMessageProcessor(rabbitMqConnectionFactory, Qname);

            IFileLogger fileLogger = new FileLogger();

            var messageHandler = new CustomMessageHandler(fileLogger, AppName, SqlConnectionString, DatabaseType.SqlServer);

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
                Console.WriteLine(AppName + "Service is running. Press Ctrl+C to stop.");
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

        }
    }
}