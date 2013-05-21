using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using Xunit;

namespace Dapper.Fluent.Tests
{
    public class Facts
    {
        private string providerName;

        public void Test_DbConnection_Should_Be_Set()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
            IDbConnection connection = factory.CreateConnection();
            IDbManager dbManager = new DbManager(connection);
            Assert.NotNull(dbManager.DbConnection);
        }
    }
}
