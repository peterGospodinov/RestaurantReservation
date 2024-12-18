﻿using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Infrastructure.Db;
using CommonServices.Infrastructure.Logging;

namespace FailService
{
    public class FailCustomMessageHandler
    {
        private readonly IFileLogger _fileLogger;
        private readonly string _appName;
        private readonly List<string> _logMessagesCollection;
        private readonly IDatabaseManager _dbManager;

        public FailCustomMessageHandler(IFileLogger fileLogger,
            string appName,
            string sqlConnectionString,
            DatabaseType databaseType)
        {
            _fileLogger = fileLogger;
            _appName = appName;
            _logMessagesCollection = new List<string>();
            _dbManager = DatabaseManagerFactory.CreateDatabaseManager(databaseType, sqlConnectionString);
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

            var isStoredSuccessfully = await StoreInDatabaseAsync(storeResultToDb, "sp_insertfailmessage");

            return null;
        }

        private async Task<bool> StoreInDatabaseAsync(StoreResultToDb model, string storedProcedureName)
        {
            try
            {
                await _dbManager.ExecuteStoredProcedureAsync(storedProcedureName, model);
                if (model.Result == 1)
                {
                    await LogAsync($"Request result inserted successfully.");
                    return true;
                }
                else
                {
                    await LogErrorAsync(model.ResultText);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
                return false;
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
