using CommonServices.Domain.Enums;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SuccessService
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
            var logMessagesCollection = new List<string>();
            logMessagesCollection.Add("Validation service started_10");
            logMessagesCollection.Add("Validation service started_20");
            logMessagesCollection.Add("Validation service started_30");
            logMessagesCollection.Add("Validation service started_40");
            logMessagesCollection.Add("Validation service started_50");
            logMessagesCollection.Add("Validation service started_60");
            logMessagesCollection.Add("Validation service started_70");
            logMessagesCollection.Add("Validation service started_80");
            logMessagesCollection.Add("Validation service started_90");
            logMessagesCollection.Add("Validation service started_100");
            logMessagesCollection.Add("Validation service started_110");
            logMessagesCollection.Add("Validation service started_120");

            Thread.Sleep(2000);
            Task.Run(() => logger?.LogAsync(logMessagesCollection)).Wait();
        }
    }
}