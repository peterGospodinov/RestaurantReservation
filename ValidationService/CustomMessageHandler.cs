using CommonServices.Domain.Enums;
using CommonServices.Infrastructure.Db;
using System.Reflection;

namespace ValidationService
{
    /*
    public class CustomMessageHandler
    {
        public async Task HandleMessageAsync(Message message)
        {
            if (ValidateMessage(message))
            {
                await StoreInDatabaseAsync(message);
                await LogMessageAsync(message);

                // Check if we need to send a response
                if (NeedsResponse(message))
                {
                    await SendResponseAsync(message);
                }
            }
            else
            {
                // Handle invalid messages, e.g., move to a dead letter queue
            }
        }
    }

    private async Task StoreInDatabaseAsync(Message message)
    {
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
       
    }
    */
}
