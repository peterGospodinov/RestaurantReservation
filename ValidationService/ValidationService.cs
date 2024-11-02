using CommonServices.Domain.Enums;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace ValidationService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFileLogger>(provider => new FileLogger())
                .BuildServiceProvider();

          
            var logger = serviceProvider.GetService<IFileLogger>();

            //Create Log messages thread-safe FIFO collection
            var logMessagesCollection = new List<string>();
            logMessagesCollection.Add("Validation service started_1");
            logMessagesCollection.Add("Validation service started_2");
            logMessagesCollection.Add("Validation service started_3");
            logMessagesCollection.Add("Validation service started_4");
            logMessagesCollection.Add("Validation service started_5");
            logMessagesCollection.Add("Validation service started_6");
            logMessagesCollection.Add("Validation service started_7");
            logMessagesCollection.Add("Validation service started_8");
            logMessagesCollection.Add("Validation service started_9");
            logMessagesCollection.Add("Validation service started_10");
            logMessagesCollection.Add("Validation service started_11");
            logMessagesCollection.Add("Validation service started_12");


            // Run logging in a new background task
            //This combination of ConcurrentQueue and SemaphoreSlim ensures that log messages are safely enqueued
            //by multiple tasks and processed in a thread-safe, FIFO manner,
            //with only one thread writing to the file at a time.
            Thread.Sleep(2000);
            Task.Run(() => logger?.LogAsync(logMessagesCollection)).Wait();
            Task.Run(() => logger?.LogAsync(new List<string> { "Another action started" }));

            //Lets do SQL things.
            string sqlConnectionString = "Server=mssql:1433;Database=Reservations;User Id=sa;Password=MyStrongS1rootP@ssword;";
            var dbManager = DatabaseManagerFactory.CreateDatabaseManager(DatabaseType.SqlServer, sqlConnectionString);

            var model = new InsertRequestResultModel
            {
                ClientName = "John Doe",
                ClientPhone = "1234567890",
                TableNumber = 5,
                DateOfReservation = DateTime.Now,
                Status = 1,
                Raw = "Sample raw data"
            };

            //insert result
            //dbManager.ExecuteStoredProcedure("sp_InsertRequestResult", model);
          
            Task.Run(() => logger?.LogAsync(new List<string> {$"Result: {model.Result}",$"Result: {model.Result}"}));

        }
    }
}