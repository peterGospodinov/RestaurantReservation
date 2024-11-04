using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;


namespace ValidationService
{
    class Program
    {
        private ConcurrentQueue<MessageModel> _messageQueue = new ConcurrentQueue<MessageModel>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10); // Controls the maximum number of concurrent tasks


        static async Task Main(string[] args)
        {
            const string AppName = "ValidationService";

            // Setup Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFileLogger>(provider => new FileLogger())
                .BuildServiceProvider();

          
            var logger = serviceProvider.GetService<IFileLogger>();

            //Create Log messages thread-safe FIFO collection
            var logMessagesCollection = new List<string>();
           
 
            
           
        

            //private ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();

            //end of initialization here

            var model = new StoreResultToDb
            {
                Raw = "Sample raw data",
                Dt = DateTime.Now,
                ValidationResult = 9               
            };

            //idva msg... dobavia se v edna colecsia... prawi all this things... i nakraq se trie.
            

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
        }       

    }
}