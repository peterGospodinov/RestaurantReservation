using CommonServices.Domain.Enums;
using System;

namespace CommonServices.Infrastructure.Db
{
    public class DatabaseManagerFactory
    {
        public static IDatabaseManager CreateDatabaseManager(DatabaseType dbType, string connectionString)
        {
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerManager(connectionString);
                case DatabaseType.PostgreSql:
                    return new PostgreSqlManager(connectionString);
                default:
                    throw new ArgumentException("Invalid database type.");
            }
        }
    }
}
