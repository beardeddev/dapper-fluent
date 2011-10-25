using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Dapper.Extensions
{
    internal class ConnectionManager
    {
        private ConnectionStringSettings _ConnectionStringSettings;

        private const string DEFAUL_PROVIDER_NAME = "System.Data.SqlClient";

        public ConnectionManager(string connectionStringName)
        {
            this._ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (this._ConnectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("Invalid connection name \"" + connectionStringName + "\"");
            }
        }

        public IDbConnection GetConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(this._ConnectionStringSettings.ProviderName);
            if (factory == null)
            {
                throw new Exception("Could not get factory for provider \"" + this._ConnectionStringSettings.ProviderName + "\"");
            }

            DbConnection dbConnection = factory.CreateConnection();
            if (dbConnection == null)
            {
                throw new Exception("Could not create connection for factory\"" + factory.GetType().FullName + "\"");
            }

            dbConnection.ConnectionString = this._ConnectionStringSettings.ConnectionString;
            dbConnection.Open();
            return dbConnection;
        }
    }
}
