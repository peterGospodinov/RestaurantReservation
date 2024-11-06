using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Domain.Queue;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;

namespace SuccessService
{
    public class SucessCustomMessageHandler
    {
        private readonly IFileLogger _fileLogger;
        private readonly string _appName;
        private readonly List<string> _logMessagesCollection;
        private readonly IDatabaseManager _dbManager;
        private readonly DatabaseType _databaseType;

        public SucessCustomMessageHandler(IFileLogger fileLogger,
            string appName,
            string sqlConnectionString,
            DatabaseType databaseType)
        {
            _fileLogger = fileLogger;
            _appName = appName;
            _logMessagesCollection = new List<string>();
            _dbManager = DatabaseManagerFactory.CreateDatabaseManager(_databaseType, sqlConnectionString);
        }


        public async Task<MessageModel> HandleMessageAsync(MessageModel message)
        {
            Console.WriteLine($"Message with Id: {message.CorrelationId} was received");
            await LogAsync($"Message with Id: {message.CorrelationId} and Content: {message.Content} was received");

            var storeResultToDb = new StoreResultToDb
            {
                Dt = DateTime.Now,
                Raw = message.Content,
                ValidationResult = message.ValidationResult,
            };

            await StoreInDatabaseAsync(storeResultToDb, "sp_InsertSucessMessage");

            string replyToQueue = QueueNames.Validation.Receive;

            return new MessageModel
            {              
                CorrelationId = message.CorrelationId,
                RoutingKey = RabbitMqRoutingKeys.Success.ToString(),
                Content = message.Content,
                ValidationResult = message.ValidationResult,     
                ReplyToQueue = replyToQueue
            };
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
