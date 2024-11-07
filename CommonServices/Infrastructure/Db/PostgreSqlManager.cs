using System.Data;
using System.Reflection;
using System;
using Npgsql;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Db
{
    public class PostgreSqlManager : IDatabaseManager
    {
        private readonly string _connectionString;

        public PostgreSqlManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExecuteStoredProcedureAsync<TModel>(string storedProcedureName, TModel model) where TModel : class
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {

                await connection.OpenAsync();
        
                using (var command = new NpgsqlCommand($"CALL {storedProcedureName.ToLower()}", connection))
                {
                    command.CommandType = CommandType.Text;

                    // Loop through model properties and add them as parameters
                    foreach (PropertyInfo prop in model.GetType().GetProperties())
                    {
                        var paramValue = prop.GetValue(model);

                        // Check if the property is intended as an output
                        if (prop.PropertyType == typeof(byte) && prop.Name == "Result" ||
                            prop.PropertyType == typeof(string) && prop.Name == "ResultText")
                        {
                            // Add output parameters
                            var npgsqlParam = new NpgsqlParameter(prop.Name.ToLower(), prop.PropertyType == typeof(byte) ? DbType.Byte : DbType.String)
                            {
                                Direction = ParameterDirection.Output,
                                Size = prop.PropertyType == typeof(string) ? 255 : 0
                            };
                            command.Parameters.Add(npgsqlParam);
                        }
                        else
                        {
                            // Add input parameters
                            command.Parameters.AddWithValue(prop.Name.ToLower(), paramValue ?? DBNull.Value);
                        }
                    }

                    try
                    {
                        // Execute the stored procedure
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    { 
                    };

                    // Retrieve output values and set them back to the model
                    foreach (NpgsqlParameter npgsqlParam in command.Parameters)
                    {
                        var prop = model.GetType().GetProperty(npgsqlParam.ParameterName);
                        if (prop != null && npgsqlParam.Direction == ParameterDirection.Output)
                        {
                            prop.SetValue(model, npgsqlParam.Value == DBNull.Value ? null : npgsqlParam.Value);
                        }
                    }
                }
            }
        }
    }
}
