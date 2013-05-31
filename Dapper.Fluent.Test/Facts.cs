using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;

using Xunit;

namespace Dapper.Fluent.Tests
{
    using Dapper.Fluent.Tests.Entities;

    public class Facts
    {
        private string providerName = "System.Data.SqlClient";
        private string connectionString = "Server=localhost;Database=Northwind;Trusted_Connection=true;Integrated Security=True";

        private IDbConnection GetConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(providerName);
            IDbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        private IDbManager GetDbManager()
        {
            IDbConnection connection = GetConnection();
            return new DbManager(connection);
        }

        [Fact]
        public void Test_DbConnection_Should_Be_Set_And_Open()
        {
            IDbManager dbManager = GetDbManager();
            Assert.NotNull(dbManager.DbConnection);
            Assert.Equal(ConnectionState.Open, dbManager.DbConnection.State);
        }

        [Fact]
        public void Test_DbTransaction_Should_Be_Opened_And_Commited()
        {
            IDbManager dbManager = GetDbManager();
            dbManager.BeginTransaction();

            Assert.NotNull(dbManager.Transaction);

            dbManager.CommitTransaction();

            Assert.Null(dbManager.Transaction);
        }

        [Fact]
        public void Test_DbTransaction_Should_Be_Opened_With_IsolationLevel_ReadUncommitted_And_Rolledback()
        {
            IDbManager dbManager = GetDbManager();
            dbManager.BeginTransaction(IsolationLevel.ReadUncommitted);

            Assert.NotNull(dbManager.Transaction);
            Assert.Equal(IsolationLevel.ReadUncommitted, dbManager.Transaction.IsolationLevel);

            dbManager.RollbackTransaction();

            Assert.Null(dbManager.Transaction);
        }

        [Fact]
        public void Test_Sql_Command_Text_Should_Be_Set()
        {
            IDbManager dbManager = GetDbManager();
            dbManager.SetCommand("SELECT * FROM [dbo].[Categories]");
            Assert.NotEmpty(dbManager.DbCommand.CommandText);
        }

        [Fact]
        public void Test_Sql_Sp_Command_Text_Should_Be_Set()
        {
            IDbManager dbManager = GetDbManager();
            dbManager.SetSpCommand("CustOrderHist");
            Assert.NotEmpty(dbManager.DbCommand.CommandText);
        }

        [Fact]
        public void Test_Parameters_Should_Be_Set()
        {
            IDbManager dbManager = GetDbManager();
            dbManager.SetSpCommand("Employee Sales by Country")
                .AddParameter("@Beginning_Date", new DateTime(1996, 07, 04))
                .AddParameter("@Ending_Date", new DateTime(1996, 07, 16))
                .ExecuteNonQuery();

            Assert.NotEmpty(dbManager.DbCommand.Parameters);
            Assert.Equal(2, dbManager.DbCommand.Parameters.Count);
        }

        [Fact]
        public void Test_Should_Execute_List()
        {
            IDbManager dbManager = GetDbManager();
            IEnumerable<CustOrderHist> result = dbManager.SetSpCommand("CustOrderHist")
                .AddParameter("@CustomerID", "ALFKI")
                .ExecuteList<CustOrderHist>();

            Assert.NotEmpty(result);
        }

        [Fact]
        public void Test_Should_Execute_Object()
        {
            IDbManager dbManager = GetDbManager();
            Category result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories] WHERE [CategoryID] = @CategoryID", new { @CategoryID = 1 })
                .ExecuteObject<Category>();

            Assert.NotNull(result);
            Assert.Equal(1, result.CategoryID);
        }

        [Fact]
        public void Test_Should_Execute_Non_Query()
        {
            IDbManager dbManager = GetDbManager();
            int result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories]")
                .ExecuteNonQuery();

            Assert.Equal(-1, result);
        }

        [Fact]
        public void Test_Should_Execute_Reader()
        {
            IDbManager dbManager = GetDbManager();
            using (IDataReader dr = dbManager.SetCommand("SELECT * FROM [dbo].[Categories]")
                                        .ExecuteReader())
            {
                Assert.NotNull(dr);
                Assert.True(dr.Read());
            }
        }

        [Fact]
        public void Test_Should_Execute_Scalar()
        {
            IDbManager dbManager = GetDbManager();
            int result = dbManager.SetCommand("SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                .ExecuteScalar<int>();
            
            Assert.Equal(8, result);
        }

        [Fact]
        public void Test_Should_Execute_Multiple()
        {
            IDbManager dbManager = GetDbManager();
            var result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories] SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                .ExecuteMultiple<Category, int>();

            Assert.NotEmpty(result.Item1);
            Assert.NotEmpty(result.Item2);

            Assert.Equal(8, result.Item1.Count());
            Assert.Equal(8, result.Item2.First());
        }

        [Fact]
        public void Test_Should_Execute_Multiple2()
        {
            IDbManager dbManager = GetDbManager();
            var result = dbManager.SetCommand(@"SELECT * FROM [dbo].[Categories] WHERE [CategoryID] = @CategoryID 
                                                SELECT * FROM [dbo].[Categories] 
                                                SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                .AddParameter("@CategoryID", 1)
                .ExecuteMultiple<Category, Category, int>();

            Assert.NotEmpty(result.Item1);
            Assert.NotEmpty(result.Item2);
            Assert.NotEmpty(result.Item3);

            Assert.Equal(1, result.Item1.First().CategoryID);
            Assert.Equal(8, result.Item2.Count());
            Assert.Equal(8, result.Item3.First());
        }

        [Fact]
        public void Test_Should_Execute_Dictionary()
        {
            IDbManager dbManager = GetDbManager();
            IDictionary result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories] WHERE [CategoryID] = @CategoryID ")
                .AddParameter("@CategoryID", 1)
                .ExecuteDictionary();

            Assert.NotNull(result);
            Assert.Contains("CategoryID", result.Keys.Cast<string>());
            Assert.Contains("CategoryName", result.Keys.Cast<string>());
            Assert.Contains("Description", result.Keys.Cast<string>());
            Assert.Contains("Picture", result.Keys.Cast<string>());
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping()
        {
            IDbManager dbManager = GetDbManager();
            IEnumerable<Product> result = dbManager.SetCommand(@"SELECT *
                          FROM [dbo].[Products] p
	                        INNER JOIN [dbo].[Categories] c ON p.CategoryID = c.CategoryID")
            .ExecuteMultiMapping<Product, Category, Product>((product, category) => { product.Category = category; return product; }, "CategoryID");

            Assert.NotEmpty(result);
            Assert.NotNull(result.First().Category);
            Assert.Equal(result.First().CategoryId, result.First().Category.CategoryID);
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping2()
        {
            IDbManager dbManager = GetDbManager();
            IEnumerable<Product> result = dbManager.SetCommand(@"SELECT *
                          FROM [dbo].[Products] p
	                        INNER JOIN [dbo].[Categories] c ON p.CategoryID = c.CategoryID
	                        INNER JOIN [dbo].[Suppliers] s ON p.SupplierID = s.SupplierID")
            .ExecuteMultiMapping<Product, Category, Supplier, Product>((product, category, supplier) => { product.Category = category; product.Supplier = supplier; return product; }, "CategoryID, SupplierID");

            Assert.NotEmpty(result);
            Assert.NotNull(result.First().Category);
            Assert.NotNull(result.First().Supplier);
            Assert.Equal(result.First().CategoryId, result.First().Category.CategoryID);
            Assert.Equal(result.First().SupplierId, result.First().Supplier.SupplierId);
        }
    }
}
