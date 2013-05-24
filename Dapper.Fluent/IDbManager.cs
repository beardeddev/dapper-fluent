using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace Dapper.Fluent
{
    /// <summary>
    /// Implement this interface to enable services and instantiation for fluent database query construction and execution over connection.
    /// </summary>
    public interface IDbManager : IDisposable
    {
        /// <summary>
        /// Gets the transaction.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Gets the db command.
        /// </summary>
        IDbCommand DbCommand { get; }

        /// <summary>
        /// Gets the db connection.
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// The parameters of the SQL statement or stored procedure. The default is an empty collection.
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Parameters { get; }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="IsolationLevel">The isolation level under which the transaction should run.</param>
        void BeginTransaction(IsolationLevel IsolationLevel);

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Executes a SQL statement against the connection and returns the result.
        /// </summary>
        /// <returns>A object returned by the query.</returns>
        object Execute();

        /// <summary>
        /// Executes SQL statement on the database and returns a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the returned collection.</typeparam>
        /// <returns>
        /// A collection of objects returned by the query.
        /// </returns>
        IEnumerable<T> ExecuteList<T>() where T : class;
        
        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery();

        /// <summary>
        /// Executes SQL statement against the connection and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the element to be the returned.</typeparam>
        /// <returns>
        /// An object returned by the query.
        /// </returns>
        T ExecuteObject<T>() where T : class;

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        IDataReader ExecuteReader();

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="IDataReader"/>  using one of the <see cref="CommandBehavior"/> values.
        /// </summary>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        IDataReader ExecuteReader(CommandBehavior behavior);

        /// <summary>
        /// Executes SQL statement against the connection and builds a <see cref="System.Collections.IDictionary"/>.
        /// </summary>
        /// <returns>A <see cref="System.Collections.IDictionary"/> object.</returns>
        IDictionary ExecuteDictionary();
        
        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple&lt;T1, T2&gt;"/> object of multiple result sets returned by the query.</returns>
        Tuple<IEnumerable<T1>, IEnumerable<T2>> ExecuteMultiple<T1, T2>();

        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <typeparam name="T3">The type of the third result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple&lt;T1, T2, T3&gt;"/> object of multiple result sets returned by the query.</returns>
        Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> ExecuteMultiple<T1, T2, T3>();

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, the parameter value, the data type, the parameter direction, and the column length.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="param">The parameter value.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <param name="size">The column length.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction, int? size);

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, the parameter value, the data type, and the parameter direction.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="param">The parameter value.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager AddParameter(string name, object value, DbType dbType, ParameterDirection direction);

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, the data type, and the parameter direction.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">One of the <see cref="System.Data.DbType"/> values.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection"/> values.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager AddParameter(string name, DbType dbType, ParameterDirection direction);

        /// <summary>
        /// Adds a parameter to the parameter collection with the parameter name, and the value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="param">The parameter value.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager AddParameter(string name, object value);
        
        /// <summary>
        /// Sets SQL statement.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager SetCommand(string commandText);

        /// <summary>
        /// Sets SQL statement and adds parameters to the parameter collection.
        /// </summary>
        /// <param name="commandText">The SQL statement text.</param>
        /// <param name="parameters">The collection of parameters associated with SQL statement.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager SetCommand(string commandText, object parameters);
        
        /// <summary>
        /// Sets the stored procedure command.
        /// </summary>
        /// <param name="commandText">The stored procedure command name.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager SetSpCommand(string commandText);

        /// <summary>
        /// Sets the stored procedure command.
        /// </summary>
        /// <param name="commandText">The stored procedure command name and adds parameters to the parameter collection.</param>
        /// <param name="parameters">The collection of parameters associated with SQL statement.</param>
        /// <returns>A <see cref="Dapper.Fluent.IDbManager"/> instance.</returns>
        IDbManager SetSpCommand(string commandText, object parameters);        
    }
}
