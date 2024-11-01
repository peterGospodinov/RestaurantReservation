using CommonServices.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;


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


            //it is ready.... here
            logMessagesCollection.Add("This is a log message10");
            logMessagesCollection.Add("This is a log message20");
            logMessagesCollection.Add("This is a log message30");
                


            // Run logging in a new background task
            Task.Run(() => logger?.LogAsync(logMessagesCollection)).Wait();
            Task.Run(() => logger?.LogAsync(new List<string> { "Another action started" }));

            //This combination of ConcurrentQueue and SemaphoreSlim ensures that log messages are safely enqueued by multiple tasks and processed in a thread-safe, FIFO manner, with only one thread writing to the file at a time.

            //Lets do SQL things.
        }
    }
}