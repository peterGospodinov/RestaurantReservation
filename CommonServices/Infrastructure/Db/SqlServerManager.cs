﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Db
{
    public class SqlServerManager : IDatabaseManager
    {
        private readonly string _connectionString;

        public SqlServerManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExecuteStoredProcedureAsync<TModel>(string storedProcedureName, TModel model) where TModel : class
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Loop through model properties and add them as parameters
                    foreach (PropertyInfo prop in model.GetType().GetProperties())
                    {
                        var paramValue = prop.GetValue(model);

                        // Check if the property is intended as an output
                        if (prop.PropertyType == typeof(byte) && prop.Name == "Result" ||
                            prop.PropertyType == typeof(string) && prop.Name == "ResultText")
                        {
                            var sqlParam = new SqlParameter(prop.Name, prop.PropertyType == typeof(byte) ? SqlDbType.TinyInt : SqlDbType.NVarChar)
                            {
                                Direction = ParameterDirection.Output,
                                Size = prop.PropertyType == typeof(string) ? 255 : 0
                            };
                            command.Parameters.Add(sqlParam);
                        }
                        else
                        {
                            command.Parameters.AddWithValue(prop.Name, paramValue ?? DBNull.Value);
                        }
                    }

                    await command.ExecuteNonQueryAsync();

                    foreach (SqlParameter sqlParam in command.Parameters)
                    {
                        var prop = model.GetType().GetProperty(sqlParam.ParameterName);
                        if (prop != null && sqlParam.Direction == ParameterDirection.Output)
                        {
                            prop.SetValue(model, sqlParam.Value == DBNull.Value ? null : sqlParam.Value);
                        }
                    }
                }
            }
        }
    }
}
