using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Domain.Queue;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ValidationService
{

    public class ValidationCustomMessageHandler
    {
        private readonly IFileLogger _fileLogger;
        private readonly string _appName;
        private readonly List<string> _logMessagesCollection;
        private readonly IDatabaseManager _dbManager;
        private readonly Validator validator;

        public ValidationCustomMessageHandler(IFileLogger fileLogger,
            string appName,
            string sqlConnectionString,
            DatabaseType databaseType)
        {
            _fileLogger = fileLogger;
            _appName = appName;
            _logMessagesCollection = new List<string>();
            _dbManager = DatabaseManagerFactory.CreateDatabaseManager(databaseType, sqlConnectionString);
            validator = new Validator();
        }

        
        public async Task<MessageModel> HandleMessageAsync(MessageModel message)
        {
            Console.WriteLine($"Message with ID: {message.CorrelationId} has been successfully received.");
            await LogAsync($"Message with Id: {message.CorrelationId} and Content: {message.Content} has been successfully received.");

            var result = validator.ValidateContent(message);
            
            var storeResultToDb = new StoreResultToDb
            {
                Dt = DateTime.Now,
                Raw = message.Content,
                ValidationResult = result.ValidationResult,            
            };
            
            if (message.RoutingKey == RabbitMqRoutingKeys.Validate.ToString())
            {
                await StoreInDatabaseAsync(storeResultToDb, "sp_InsertValidatedMessage");

                string forwardToQueue = message.ForwardToQueue;
                string routingKey = message.RoutingKey;

                if (result.ValidationResult == 0)
                {
                    forwardToQueue = QueueNames.Fail.Receive;
                    routingKey = RabbitMqRoutingKeys.Fail.ToString();
                }
                else if (result.ValidationResult == 9)
                {
                    forwardToQueue = QueueNames.Success.Receive;
                    routingKey = RabbitMqRoutingKeys.Success.ToString();
                }

                return new MessageModel
                {
                    Content = message.Content,
                    CorrelationId = message.CorrelationId,
                    RoutingKey = routingKey,
                    ValidationResult = result.ValidationResult,     
                    ReplyToQueue = message.ReplyToQueue,
                    ForwardToQueue = forwardToQueue
                };
            }
            else if (message.RoutingKey == RabbitMqRoutingKeys.Success.ToString())
            {
                await StoreInDatabaseAsync(storeResultToDb, "sp_InsertSucessResponceMessage");
                return null;
            }
            else
            {
                return null;
            }
         
        } 
        

        private async Task StoreInDatabaseAsync(StoreResultToDb model, string storedProcedureName)
        {
            try
            {             
                await _dbManager.ExecuteStoredProcedureAsync(storedProcedureName, model);
                if (model.Result == 1)
                {

                    await LogAsync($"Request result inserted successfully.");
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

        private async Task LogAsync(string message)
        {
            string logMessage = $"{_appName}: {message.Trim()}";
            await _fileLogger.LogAsync(new List<string> { logMessage });
        }

        private async Task LogErrorAsync(string message)
        {
            string errorMessage = $"{_appName}: Error - {message.Trim()}";
            await _fileLogger.LogAsync(new List<string> { errorMessage });
        }
    }
}
