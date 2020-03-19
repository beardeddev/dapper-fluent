using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Threading.Tasks;

namespace Dapper.Fluent
{
    /// <summary>
    /// Implement this interface to enable services and instantiation for fluent database query construction and execution over connection.
    /// </summary>
    public partial interface IDbManager : IDisposable
    {
        /// <summary>
        /// Executes SQL statement on the database and returns a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the returned collection.</typeparam>
        /// <returns>
        /// A collection of objects returned by the query.
        /// </returns>
        Task<IEnumerable<T>> ExecuteListAsync<T>();

        /// <summary>
        /// Executes SQL statement against the connection and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the element to be the returned.</typeparam>
        /// <returns>
        /// An object returned by the query.
        /// </returns>
        Task<T> ExecuteObjectAsync<T>();

        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple&lt;T1, T2&gt;"/> object of multiple result sets returned by the query.</returns>
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> ExecuteMultipleAsync<T1, T2>();

        /// <summary>
        /// Executes SQL statement against the connection and builds multiple result sets.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set elements to be the returned.</typeparam>
        /// <typeparam name="T2">The type of the second result set elements to be the returned.</typeparam>
        /// <typeparam name="T3">The type of the third result set elements to be the returned.</typeparam>
        /// <returns>A <see cref="System.Tuple&lt;T1, T2, T3&gt;"/> object of multiple result sets returned by the query.</returns>
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> ExecuteMultipleAsync<T1, T2, T3>();

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, TResult>(Func<T1, T2, TResult> map, string splitOn);

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>The result object with related associations.</returns>
        Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> map, string splitOn);

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
        /// <returns>The result object with related associations.</returns>
        Task<IEnumerable<TResult>> ExecuteMappingSync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> map, string splitOn);

        /// <summary>
        /// Executes SQL statement against the connection and maps a single row to multiple objects.
        /// </summary>
        /// <typeparam name="T1">The type of the first result set.</typeparam>
        /// <typeparam name="T2">The type of the second result set.</typeparam>
        /// <typeparam name="T3">The type of the third result set.</typeparam>
        /// <typeparam name="T4">The type of the fourth result set.</typeparam>
        /// <typeparam name="T5">The type of the fifth result set.</typeparam>
        /// <typeparam name="TResult">The type of the result set elements to be the returned.</typeparam>
        /// <param name="map">The mapping function that encapsulates a method that has three parameters and returns a value of the type specified by the TResult parameter..</param>
        /// <param name="splitOn">The name of the field result set should split and read the second object from (default: id).</param>
        /// <returns>
        /// The result object with related associations.
        /// </returns>
        Task<IEnumerable<TResult>> ExecuteMappinAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> map, string splitOn = "Id");

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
        /// <returns>The result object with related associations.</returns>
        Task<IEnumerable<TResult>> ExecuteMappingAsync<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> map, string splitOn);

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        Task<int> ExecuteAsync();        
    }
}
