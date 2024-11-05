using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ValidationService
{

    public class CustomMessageHandler
    {
        private readonly IFileLogger _fileLogger;
        private readonly string _appName;
        private readonly List<string> _logMessagesCollection;
        private readonly IDatabaseManager _dbManager;
        private readonly Validator validator;

        public CustomMessageHandler(IFileLogger fileLogger,
            string sqlConnectionString,
            string appName)
        {
            _fileLogger = fileLogger;
            _appName = appName;
            _logMessagesCollection = new List<string>();
            _dbManager = DatabaseManagerFactory.CreateDatabaseManager(DatabaseType.SqlServer, sqlConnectionString);
            validator = new Validator();
        }

        
        public async Task HandleMessageAsync(MessageModel message)
        {
            Console.WriteLine("Message was received.");
            var result = validator.ValidateContent(message);
            Console.WriteLine("Message was vaidated.");

            var storeResultToDb = new StoreResultToDb
            {
                Dt = DateTime.Now,
                Raw = message.Content,
                ValidationResult = result.ValidationResult,            
            };

            await StoreInDatabaseAsync(storeResultToDb);

        } 
        

        private async Task StoreInDatabaseAsync(StoreResultToDb model)
        {
            try
            {             
                await _dbManager.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model);
                if (model.Result == 1)
                {

                    await LogSuccessAsync($"Request result inserted successfully.");
                }
                else
                {
                    await LogErrorAsync(model.ResultText);
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
            };


        }

        private async Task LogMessageAsync(MessageModel message)
        {
            // Log any additional information about the message processing if needed
            await LogSuccessAsync($"{_appName}: Message processed - {message.Content}");
        }

        private async Task LogSuccessAsync(string message)
        {
            string errorMessage = $"{_appName}: Sucess - {message.Trim()}";
            await _fileLogger.LogAsync(new List<string> { errorMessage });
        }

        private async Task LogErrorAsync(string message)
        {
            string errorMessage = $"{_appName}: Error - {message.Trim()}";
            await _fileLogger.LogAsync(new List<string> { errorMessage });
        }
    }
}
