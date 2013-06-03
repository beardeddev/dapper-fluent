using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Configuration;

using Xunit;

namespace Dapper.Fluent.Tests
{
    using Dapper.Fluent.Tests.Entities;

    public class Facts
    {
        private readonly ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Dapper.Fluent.Tests.ConnectionString"];

        private IDbConnection GetConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
            IDbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString.ConnectionString;
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
            using (IDbManager dbManager = GetDbManager())
            {
                Assert.NotNull(dbManager.DbConnection);
                Assert.Equal(ConnectionState.Open, dbManager.DbConnection.State);
            }
        }

        [Fact]
        public void Test_DbTransaction_Should_Be_Opened_And_Commited()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                dbManager.BeginTransaction();

                Assert.NotNull(dbManager.Transaction);

                dbManager.CommitTransaction();

                Assert.Null(dbManager.Transaction);
            }
        }

        [Fact]
        public void Test_DbTransaction_Should_Be_Opened_With_IsolationLevel_ReadUncommitted_And_Rolledback()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                dbManager.BeginTransaction(IsolationLevel.ReadUncommitted);

                Assert.NotNull(dbManager.Transaction);
                Assert.Equal(IsolationLevel.ReadUncommitted, dbManager.Transaction.IsolationLevel);

                dbManager.RollbackTransaction();

                Assert.Null(dbManager.Transaction);
            }
        }

        [Fact]
        public void Test_Should_Execute_List()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Category> result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories]")
                    .ExecuteList<Category>();

                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public void Test_Should_Execute_List_With_Parameters()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<CustOrderHist> result = dbManager.SetSpCommand("CustOrderHist")
                    .SetParameter("@CustomerID", "ALFKI")
                    .ExecuteList<CustOrderHist>();

                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public void Test_Should_Execute_List_With_Dynamic_Parameters()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<EmployeeSalesByCountryResult> result = dbManager.SetSpCommand("Employee Sales by Country")
                    .SetParameters(new { 
                        @Beginning_Date = new DateTime(1996, 07, 04), @Ending_Date = new DateTime(1996, 07, 16) 
                    }).ExecuteList<EmployeeSalesByCountryResult>();

                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public void Test_Should_Execute_Object()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                Category result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories] WHERE [CategoryID] = @CategoryID", new { @CategoryID = 1 })
                    .ExecuteObject<Category>();

                Assert.NotNull(result);
                Assert.Equal(1, result.CategoryID);
            }
        }

        [Fact]
        public void Test_Should_Execute_List_With_Mixed_Parameters()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<EmployeeSalesByCountryResult> result = dbManager.SetSpCommand("Employee Sales by Country", new { 
                        @Beginning_Date = new DateTime(1996, 07, 04) 
                    }).SetParameter("@Ending_Date", new DateTime(1996, 07, 16))
                    .ExecuteList<EmployeeSalesByCountryResult>();

                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public void Test_Should_Execute_Scalar()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                int result = dbManager.SetCommand("SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                    .ExecuteObject<int>();

                Assert.Equal(8, result);
            }
        }

        [Fact]
        public void Test_Should_Execute_NonQuery()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                int result = dbManager.SetCommand("SET NOCOUNT ON SELECT * FROM [dbo].[Categories]")
                    .Execute();

                Assert.Equal(-1, result);
            }
        }

        [Fact]
        public void Test_Should_Pass_Output_Parameter()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Category> result = dbManager.SetCommand("SET NOCOUNT ON SELECT * FROM [dbo].[Categories] SELECT @TotalCount = COUNT(CategoryID) FROM [dbo].[Categories]")
                    .SetOutputParameter("@TotalCount", DbType.Int32)
                    .ExecuteList<Category>();

                Assert.NotEmpty(result);
                Assert.Equal(8, dbManager.GetParameterValue<int>("@TotalCount"));
            }
        }

        [Fact]
        public void Test_Should_Execute_Multiple()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                var result = dbManager.SetCommand("SELECT * FROM [dbo].[Categories] SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                    .ExecuteMultiple<Category, int>();

                Assert.NotEmpty(result.Item1);
                Assert.NotEmpty(result.Item2);

                Assert.Equal(8, result.Item1.Count());
                Assert.Equal(8, result.Item2.First());
            }
        }

        [Fact]
        public void Test_Should_Execute_Multiple2()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                var result = dbManager.SetCommand(@"SELECT * FROM [dbo].[Categories] WHERE [CategoryID] = @CategoryID 
                                                SELECT * FROM [dbo].[Categories] 
                                                SELECT COUNT(CategoryID) FROM [dbo].[Categories]")
                    .SetParameter("@CategoryID", 1)
                    .ExecuteMultiple<Category, Category, int>();

                Assert.NotEmpty(result.Item1);
                Assert.NotEmpty(result.Item2);
                Assert.NotEmpty(result.Item3);

                Assert.Equal(1, result.Item1.First().CategoryID);
                Assert.Equal(8, result.Item2.Count());
                Assert.Equal(8, result.Item3.First());
            }
        }        

        [Fact]
        public void Test_Should_Execute_MultiMapping()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Product> result = dbManager.SetCommand(@"SELECT *
                          FROM [dbo].[Products] p
	                        INNER JOIN [dbo].[Categories] c ON p.CategoryID = c.CategoryID")
                .ExecuteMapping<Product, Category, Product>(
                    (product, category) => { 
                        product.Category = category; 
                        return product; 
                    }, 
                    "CategoryID"
                );

                Assert.NotEmpty(result);
                Assert.NotNull(result.First().Category);
                Assert.Equal(result.First().CategoryId, result.First().Category.CategoryID);
            }
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping2()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Product> result = dbManager.SetCommand(@"SELECT *
                          FROM [dbo].[Products] p
	                        INNER JOIN [dbo].[Categories] c ON p.CategoryID = c.CategoryID
	                        INNER JOIN [dbo].[Suppliers] s ON p.SupplierID = s.SupplierID")
                .ExecuteMapping<Product, Category, Supplier, Product>(
                    (product, category, supplier) => { 
                        product.Category = category; 
                        product.Supplier = supplier; 
                        return product; 
                    }, 
                    "CategoryID, SupplierID"
                );

                Assert.NotEmpty(result);
                Assert.NotNull(result.First().Category);
                Assert.NotNull(result.First().Supplier);
                Assert.Equal(result.First().CategoryId, result.First().Category.CategoryID);
                Assert.Equal(result.First().SupplierId, result.First().Supplier.SupplierId);
            }
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping3()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Order> result = dbManager.SetCommand(@"SELECT *
                      FROM [dbo].[Orders] o
                      INNER JOIN [dbo].[Customers] c ON o.CustomerID = c.CustomerID
                      INNER JOIN [dbo].[Employees] e ON o.EmployeeID = e.EmployeeID
                      INNER JOIN [dbo].[Shippers] s ON o.ShipVia = s.ShipperID")
                .ExecuteMapping<Order, Customer, Employee, Shipper, Order>(
                    (order, customer, employee, shipper) => { 
                        order.Customer = customer; 
                        order.Employee = employee; 
                        order.Shipper = shipper; 
                        return order; 
                    }, 
                    "CustomerID, EmployeeID, ShipperID"
                );

                Assert.NotEmpty(result);
                Assert.NotNull(result.First().Customer);
                Assert.NotNull(result.First().Employee);
                Assert.NotNull(result.First().Shipper);
                Assert.Equal(result.First().CustomerId, result.First().Customer.CustomerId);
                Assert.Equal(result.First().EmployeeId, result.First().Employee.EmployeeId);
                Assert.Equal(result.First().ShipVia, result.First().Shipper.ShipperId);
            }
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping4()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Order> result = dbManager.SetCommand(@"SELECT *
                      FROM [dbo].[Orders] o
                      INNER JOIN [dbo].[Customers] c ON o.CustomerID = c.CustomerID
                      INNER JOIN [dbo].[Employees] e ON o.EmployeeID = e.EmployeeID
                      INNER JOIN [dbo].[Shippers] s ON o.ShipVia = s.ShipperID
                      LEFT JOIN [dbo].[Employees] e2 ON e.ReportsTo = e2.EmployeeID")
                .ExecuteMapping<Order, Customer, Employee, Shipper, Employee, Order>(
                    (order, customer, employee, shipper, boss) => { 
                        order.Customer = customer; 
                        order.Employee = employee; 
                        order.Shipper = shipper; 
                        order.Employee.Boss = boss; 
                        return order; 
                    }, 
                    "CustomerID, EmployeeID, ShipperID, EmployeeID"
                );

                Assert.NotEmpty(result);
                Assert.NotNull(result.First().Customer);
                Assert.NotNull(result.First().Employee);
                Assert.NotNull(result.First().Shipper);
                Assert.NotNull(result.First().Employee.Boss);
                Assert.Equal(result.First().CustomerId, result.First().Customer.CustomerId);
                Assert.Equal(result.First().EmployeeId, result.First().Employee.EmployeeId);
                Assert.Equal(result.First().ShipVia, result.First().Shipper.ShipperId);
                Assert.Equal(result.First().Employee.ReportsTo, result.First().Employee.Boss.EmployeeId);
            }
        }

        [Fact]
        public void Test_Should_Execute_MultiMapping5()
        {
            using (IDbManager dbManager = GetDbManager())
            {
                IEnumerable<Order> result = dbManager.SetCommand(@"SELECT *
                      FROM [dbo].[Orders] o
                      INNER JOIN [dbo].[Customers] c ON o.CustomerID = c.CustomerID
                      INNER JOIN [dbo].[Employees] e ON o.EmployeeID = e.EmployeeID
                      INNER JOIN [dbo].[Shippers] s ON o.ShipVia = s.ShipperID
                      LEFT JOIN [dbo].[Employees] e2 ON e.ReportsTo = e2.EmployeeID
                      INNER JOIN [dbo].[Order Details] od ON o.OrderID = od.OrderID")
                .ExecuteMapping<Order, Customer, Employee, Shipper, Employee, OrderDetails, Order>(
                    (order, customer, employee, shipper, boss, details) =>
                    {
                        order.Customer = customer;
                        order.Employee = employee;
                        order.Shipper = shipper;
                        order.Employee.Boss = boss;
                        order.Details = details;
                        return order;
                    },
                    "CustomerID, EmployeeID, ShipperID, EmployeeID, OrderID"
                );

                Assert.NotEmpty(result);
                Assert.NotNull(result.First().Details);
                Assert.NotNull(result.First().Customer);
                Assert.NotNull(result.First().Employee);
                Assert.NotNull(result.First().Shipper);
                Assert.NotNull(result.First().Employee.Boss);
                Assert.Equal(result.First().CustomerId, result.First().Customer.CustomerId);
                Assert.Equal(result.First().EmployeeId, result.First().Employee.EmployeeId);
                Assert.Equal(result.First().ShipVia, result.First().Shipper.ShipperId);
                Assert.Equal(result.First().Employee.ReportsTo, result.First().Employee.Boss.EmployeeId);
            }
        }
    }
}
