using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

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
        private DynamicParameters parameters;
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
            this.DbCommand = connection.CreateCommand();
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

        /// <summary>
        /// Gets the db command.
        /// </summary>
        public IDbCommand DbCommand { get; private set; }

        /// <summary>
        /// The parameters of the SQL statement or stored procedure. 
        /// The default is an empty collection.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Parameters
        {
            get
            {
                foreach (string name in this.parameters.ParameterNames)
                    yield return new KeyValuePair<string, object>(name, this.parameters.Get<object>(name));
            }
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
        /// Executes a SQL statement against the connection and returns the result.
        /// </summary>
        /// <returns>
        /// A object returned by the query.
        /// </returns>
        public object Execute()
        {
            return this.DbConnection.Query<Object>(DbCommand.CommandText, parameters, Transaction, this.buffered, this.DbCommand.CommandTimeout, DbCommand.CommandType)
                .FirstOrDefault();
        }

        /// <summary>
        /// Executes SQL statement on the database and returns a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the returned collection.</typeparam>
        /// <returns>
        /// A collection of objects returned by the query.
        /// </returns>
        public IEnumerable<T> ExecuteList<T>() where T : class
        {
            return this.DbConnection.Query<T>(DbCommand.CommandText, parameters, Transaction, this.buffered, this.DbCommand.CommandTimeout, DbCommand.CommandType);
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public int ExecuteNonQuery()
        {
            return this.DbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes SQL statement against the connection and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the element to be the returned.</typeparam>
        /// <returns>
        /// An object returned by the query.
        /// </returns>
        public T ExecuteObject<T>() where T : class
        {
            return this.DbConnection.Query<T>(DbCommand.CommandText, parameters, Transaction, true, this.DbCommand.CommandTimeout, DbCommand.CommandType).FirstOrDefault();
        }

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IDataReader"/> object.
        /// </returns>
        public IDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="IDataReader"/>  using one of the <see cref="CommandBehavior"/> values.
        /// </summary>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <returns>
        /// A <see cref="IDataReader"/> object.
        /// </returns>
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return this.DbCommand.ExecuteReader(behavior);
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
            using (var rs = this.DbConnection.QueryMultiple(DbCommand.CommandText, this.parameters, this.Transaction, this.DbCommand.CommandTimeout, DbCommand.CommandType))
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
            using (var rs = this.DbConnection.QueryMultiple(DbCommand.CommandText, this.parameters, this.Transaction, this.DbCommand.CommandTimeout, DbCommand.CommandType))
            {
                IEnumerable<T1> item1 = rs.Read<T1>();
                IEnumerable<T2> item2 = rs.Read<T2>();
                IEnumerable<T3> item3 = rs.Read<T3>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>(item1, item2, item3);
            }
        }

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Collections.IDictionary"/> object.
        /// </returns>
        public virtual IDictionary ExecuteDictionary()
        {
            IDictionary result = new Dictionary<string, object>();

            using (IDataReader dr = this.ExecuteReader())
            {
                if (dr.Read())
                {
                    foreach (DataRow row in dr.GetSchemaTable().Rows)
                    {
                        string key = row["ColumnName"].ToString();
                        result.Add(key, dr[key]);
                    }
                }
            }

            return result;
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
        public IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction, int? size)
        {
            this.parameters.Add(name, value, dbType, direction, size);
            return this;
        }

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, parameter param, the data type, and the parameter direction.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction)
        {
            return this.AddParameter(name, value, dbType, direction, null);
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
        public IDbManager AddParameter(string name, DbType dbType, ParameterDirection direction)
        {
            return this.AddParameter(name, null, dbType, direction, null);
        }

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, and param.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>
        /// A <see cref="Dapper.Fluent.IDbManager"/> instance.
        /// </returns>
        public IDbManager AddParameter(string name, object value)
        {
            this.parameters.Add(name, value);
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
            this.DbCommand.CommandText = commandText;
            this.DbCommand.CommandType = CommandType.Text;
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
            this.parameters = this.CreateParameters(parameters);
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
            this.DbCommand.CommandText = commandText;
            this.DbCommand.CommandType = CommandType.StoredProcedure;
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
            this.parameters = this.CreateParameters(parameters);
            return this.SetSpCommand(commandText);
        } 
        #endregion

        #region Private methods
        private DynamicParameters CreateParameters(object param)
        {
            return new DynamicParameters(param);
        } 
        #endregion
    }
}
