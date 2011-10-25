using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data.Common;

using Dapper;

namespace Dapper.Extensions
{
    public class DbManager : IDisposable
    {
        private IDbConnection _Connection;
        private IDbTransaction _Transaction;
        private ConnectionManager _ConnectionManager;

        private string _CommandText;        
        private CommandType _CommandType;
        private DynamicParameters _Parameters;                
        
        public DbManager()
        {
            if (ConfigurationManager.ConnectionStrings.Count <= 0)
            {
                throw new ConfigurationErrorsException("No connections found in application configuration.");
            }

            this._ConnectionManager = new ConnectionManager(ConfigurationManager.ConnectionStrings[0].Name);
            this._Connection = this._ConnectionManager.GetConnection();
        }            

        public DbManager(string connectionStringName)
        {
            this._ConnectionManager = new ConnectionManager(connectionStringName);
            this._Connection = this._ConnectionManager.GetConnection();
        }

        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void BeginTransaction(IsolationLevel IsolationLevel)
        {
            this._Transaction = this._Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_Transaction != null)
                _Transaction.Commit();
        }

        public void Rollback()
        {
            if(_Transaction != null)
                _Transaction.Rollback();
        }    

        public DbManager SetCommand(string commandText)
        {
            this._CommandText = commandText;
            this._CommandType = CommandType.Text;
            return this;
        }

        public DbManager SetSpCommand(string commandText)
        {
            this._CommandText = commandText;
            this._CommandType = CommandType.StoredProcedure;
            return this;
        }

        public DbManager SetCommand(string commandText, object parameters)
        {
            this._Parameters = this.CreateParameters(parameters);
            return SetCommand(commandText, parameters);
        }

        public DbManager SetSpCommand(string commandText, DynamicParameters parameters)            
        {
            this._Parameters = parameters;
            return SetSpCommand(commandText, parameters);
        }

        public DbManager SetCommand(string commandText, DynamicParameters parameters)
        {
            this._Parameters = parameters;
            return SetCommand(commandText, parameters);
        }

        public DbManager SetSpCommand(string commandText, object parameters)
        {
            this._Parameters = this.CreateParameters(parameters);
            return SetSpCommand(commandText, parameters);
        }

        public List<T> ExecuteList<T>()
        {
            return this._Connection.Query<T>(_CommandText, _Parameters, _Transaction, true, null, _CommandType).ToList();
        }

        public T ExecuteObject<T>()
        {
            return this._Connection.Query<T>(_CommandText, _Parameters, _Transaction, true, null, _CommandType).FirstOrDefault();
        }

        public int ExecuteNonQuery()
        {          
            return this._Connection.Execute(_CommandText, _Parameters, _Transaction, null, _CommandType);
        }

        public T ExecuteScalar<T>()
            where T : struct
        {
            return this._Connection.Query<T>(_CommandText, _Parameters, _Transaction, true, null, _CommandType).FirstOrDefault();
        }

        public object ExecuteScalar()
        {
            return this._Connection.Query<Object>(_CommandText, _Parameters, _Transaction, true, null, _CommandType).FirstOrDefault();
        }

        public Tuple<List<T1>, List<T2>> ExecuteMultiple<T1, T2>()
        {
            using (var rs = this._Connection.QueryMultiple(_CommandText, _Parameters, null, null, CommandType.StoredProcedure))
            {
                List<T1> item1 = rs.Read<T1>().ToList();
                List<T2> item2 = rs.Read<T2>().ToList();

                return new Tuple<List<T1>, List<T2>>(item1, item2);
            }
        }

        public Tuple<List<T1>, List<T2>, List<T3>> ExecuteMultiple<T1, T2, T3>()
        {
            using (var rs = this._Connection.QueryMultiple(_CommandText, _Parameters, null, null, CommandType.StoredProcedure))
            {
                List<T1> item1 = rs.Read<T1>().ToList();
                List<T2> item2 = rs.Read<T2>().ToList();
                List<T3> item3 = rs.Read<T3>().ToList();

                return new Tuple<List<T1>, List<T2>, List<T3>>(item1, item2, item3);
            }
        }

        public void Dispose()
        {
            if (_Connection != null && _Connection.State == ConnectionState.Open)
            {
                _Connection.Close();
            }
            _Connection.Dispose();
        }

        public DynamicParameters CreateParameters(object value)
        {
            return new DynamicParameters(value);
        }

    }
}
