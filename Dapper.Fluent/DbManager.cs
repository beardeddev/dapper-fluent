using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Data.Common;
using static Dapper.SqlMapper;
using System.Threading.Tasks;

namespace Dapper.Fluent
{
    /// <summary>
    /// Provides support for fluent API query construction and execution over database connection.
    /// </summary>
    public class DbManager : IDbManager
    {
        #region Private members        
        private bool disposed;
        private bool buffered = true;
        private string commandText;
        private int? commandTimeout = null;
        private DynamicParameters parameters;
        private CommandType commandType;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="DbManager"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DbManager(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.State != ConnectionState.Open)
                connection.Open();

            this.DbConnection = connection;
            this.parameters = new DynamicParameters();
        }
        #endregion

        #region Transactions support implementation
        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        public virtual void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="IsolationLevel">The isolation level under which the transaction should run.</param>
        public virtual void BeginTransaction(IsolationLevel isolationLevel)
        {
            this.Transaction = this.DbConnection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public virtual void CommitTransaction()
        {
            if (Transaction != null)
                Transaction.Commit();

            Transaction = null;
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public virtual void RollbackTransaction()
        {
            if (Transaction != null)
                Transaction.Rollback();

            Transaction = null;
        }
        #endregion

        #region Public readonly members
        /// <summary>
        /// Gets the transaction.
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        public IDbConnection DbConnection { get; private set; }
        #endregion

        #region Public members
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <typeparam name="T">The type of parameter value.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <returns>The value of parameter.</returns>
        public T GetParameterValue<T>(string name)
        {
            return this.parameters.Get<T>(name);
        }
        #endregion

        #region IDisposable members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (this.DbConnection != null)
                    {
                        if (this.DbConnection.State == ConnectionState.Open)
                            this.DbConnection.Close();

                        this.DbConnection.Dispose();
                    }
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DbManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~DbManager()
        {
            Dispose(false);
        }
        #endregion

        #region Query execution methods
        /// <summary>
        /// Executes SQL statement on the database and returns a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the returned collection.</typeparam>
        /// <returns>
        /// A collection of objects returned by the query.
        /// </returns>
        public IEnumerable<T> ExecuteList<T>()
        {
            return this.DbConnection.Query<T>(this.commandText, this.parameters, this.Transaction, this.buffered, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes SQL statement against the connection and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the element to be the returned.</typeparam>
        /// <returns>
        /// An object returned by the query.
        /// </returns>
        public T ExecuteObject<T>()
        {
            return this.DbConnection.Query<T>(this.commandText, this.parameters, this.Transaction, this.buffered, this.commandTimeout, this.commandType).FirstOrDefault();
        }

        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <returns>
        /// A <see cref="System.Tuple&lt;T1, T2&gt;"/> object of multiple result sets returned by the query.
        /// </returns>
        public Tuple<IEnumerable<T1>, IEnumerable<T2>> ExecuteMultiple<T1, T2>()
        {
            using (var rs = this.DbConnection.QueryMultiple(commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType))
            {
                IEnumerable<T1> item1 = rs.Read<T1>();
                IEnumerable<T2> item2 = rs.Read<T2>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
            }
        }

        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <typeparam name="T3">The type of the third result set elements to be the returned.</typeparam>
        /// <returns>
        /// A <see cref="System.Tuple&lt;T1, T2, T3&gt;"/> object of multiple result sets returned by the query.
        /// </returns>
        public virtual Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> ExecuteMultiple<T1, T2, T3>()
        {
            using (var rs = this.DbConnection.QueryMultiple(this.commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType))
            {
                IEnumerable<T1> item1 = rs.Read<T1>();
                IEnumerable<T2> item2 = rs.Read<T2>();
                IEnumerable<T3> item3 = rs.Read<T3>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>(item1, item2, item3);
            }
        }

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        public virtual IEnumerable<TResult> ExecuteMapping<T1, T2, TResult>(Func<T1, T2, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.Query<T1, T2, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        public virtual IEnumerable<TResult> ExecuteMapping<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.Query<T1, T2, T3, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        public virtual IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.Query<T1, T2, T3, T4, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="T5">The type of the fourth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        public virtual IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.Query<T1, T2, T3, T4, T5, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="T5">The type of the fifth result set.</typeparam>
        /// <typeparam name="T6">The type of the sixth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        public virtual IEnumerable<TResult> ExecuteMapping<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.Query<T1, T2, T3, T4, T5, T6, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public virtual int Execute()
        {
            return this.DbConnection.Execute(this.commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement on the database and returns a collection of objects.</summary>
        /// <typeparam name="T">The type of the elements in the returned collection.</typeparam>
        /// <returns>A collection of objects returned by the query.</returns>
        public Task<IEnumerable<T>> ExecuteListAsync<T>()
        {
            return this.DbConnection.QueryAsync<T>(this.commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and returns the result.</summary>
        /// <typeparam name="T">The type of the element to be the returned.</typeparam>
        /// <returns>An object returned by the query.</returns>
        public Task<T> ExecuteObjectAsync<T>()
        {
            return this.DbConnection.QueryFirstOrDefaultAsync<T>(this.commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and builds multiple result sets.</summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple<T1, T2>">System.Tuple&lt;T1, T2&gt;</see> object of multiple result sets returned by the query.</returns>
        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> ExecuteMultipleAsync<T1, T2>()
        {
            using (var rs = await this.DbConnection.QueryMultipleAsync(commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType))
            {
                IEnumerable<T1> item1 = rs.Read<T1>();
                IEnumerable<T2> item2 = rs.Read<T2>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
            }
        }

        /// <summary>Executes SQL statement against the connection and builds multiple result sets.</summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <typeparam name="T3">The type of the third result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple<T1, T2, T3>">System.Tuple&lt;T1, T2, T3&gt;</see> object of multiple result sets returned by the query.</returns>
        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> ExecuteMultipleAsync<T1, T2, T3>()
        {
            using (var rs = await this.DbConnection.QueryMultipleAsync(commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType))
            {
                IEnumerable<T1> item1 = rs.Read<T1>();
                IEnumerable<T2> item2 = rs.Read<T2>();
                IEnumerable<T3> item3 = rs.Read<T3>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>(item1, item2, item3);
            }
        }

        /// <summary>Executes SQL statement against the connection and maps a single row to multiple objects.</summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        public Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, TResult>(Func<T1, T2, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.QueryAsync<T1, T2, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and maps a single row to multiple objects.</summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        public Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.QueryAsync<T1, T2, T3, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and maps a single row to multiple objects.</summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        public Task<IEnumerable<TResult>> ExecuteMappingSync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.QueryAsync<T1, T2, T3, T4, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and maps a single row to multiple objects.</summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="T5">The type of the fifth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        public Task<IEnumerable<TResult>> ExecuteMappinAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> map, string splitOn = "Id")
        {
            return this.DbConnection.QueryAsync<T1, T2, T3, T4, T5, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes SQL statement against the connection and maps a single row to multiple objects.</summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="T5">The type of the fifth result set.</typeparam>
        /// <typeparam name="T6">The type of the sixth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        public Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> map, string splitOn)
        {
            return this.DbConnection.QueryAsync<T1, T2, T3, T4, T5, T6, TResult>(this.commandText, map, this.parameters, this.Transaction, this.buffered, splitOn, this.commandTimeout, this.commandType);
        }

        /// <summary>Executes a SQL statement against the connection and returns the number of rows affected.</summary>
        /// <returns>The number of rows affected.</returns>
        public Task<int> ExecuteAsync()
        {
            return this.DbConnection.ExecuteAsync(this.commandText, this.parameters, this.Transaction, this.commandTimeout, this.commandType);
        }
        #endregion

        #region Fluent API
        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, parameter value, the data type, the parameter direction, and the column length.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <param name="size">The column length.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetParameter(string name, object value, DbType dbType, ParameterDirection direction, int? size)
        {
            this.parameters.Add(name, value, dbType, direction, size);
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, parameter value, the data type, and the parameter direction.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetParameter(string name, object value, DbType dbType, ParameterDirection direction)
        {
            return this.SetParameter(name, value, dbType, direction, null);
        }

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, the data type, and the parameter direction.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetParameter(string name, DbType dbType, ParameterDirection direction)
        {
            return this.SetParameter(name, null, dbType, direction, null);
        }

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetParameter(string name, object value)
        {
            return this.SetParameter(name, value, SqlMapper.GetDbType(value), ParameterDirection.Input, null);
        }

        /// <summary>
        /// Construct a parameter from object and adds a parameters to the parameter collection.
        /// </summary>
        /// <param name="value">Can be an anonymous type or a DynamicParameters bag.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        public IDbManager SetParameters(object value)
        {
            this.parameters.AddDynamicParams(value);
            return this;
        }

        /// <summary>
        /// Sets SQL statement.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetCommand(string commandText)
        {
            this.commandText = commandText;
            this.commandType = CommandType.Text;
            return this;
        }

        /// <summary>
        /// Sets SQL statement and adds parameters to the parameter collection.
        /// </summary>
        /// <param name="commandText">The SQL statement text.</param>
        /// <param name="parameters">The collection of parameters associated with SQL statement.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetCommand(string commandText, object parameters)
        {
            this.parameters.AddDynamicParams(parameters);
            return this.SetCommand(commandText);
        }

        /// <summary>
        /// Sets the stored procedure command.
        /// </summary>
        /// <param name="commandText">The stored procedure command name.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetSpCommand(string commandText)
        {
            this.commandText = commandText;
            this.commandType = CommandType.StoredProcedure;
            return this;
        }

        /// <summary>
        /// Sets the stored procedure command.
        /// </summary>
        /// <param name="commandText">The stored procedure command name and adds parameters to the parameter collection.</param>
        /// <param name="parameters">The collection of parameters associated with SQL statement.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager SetSpCommand(string commandText, object parameters)
        {
            this.parameters.AddDynamicParams(parameters);
            return this.SetSpCommand(commandText);
        }        
        #endregion
    }
}
