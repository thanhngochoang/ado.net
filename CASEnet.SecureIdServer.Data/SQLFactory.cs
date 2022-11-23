using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CASEnet.SecureIdServer.Data
{
    public abstract class SQLFactory : IDisposable //,ISqlFactory
    {
        public SQLFactory()
        {
        }


        protected abstract IDbConnection InitConnectionString();
        protected abstract IDbCommand InitCommand(string commandText, CommandType commandType, object objParameters);
        protected abstract IEnumerable<object> MappingPrams(object objParameters);
        protected delegate void DbLogEvent(Exception ex, IDbCommand cmd, string createdByIP, string requestUrl, string createBy);
        protected DbLogEvent dbLogger;
        private IDbTransaction _transaction;
        private IDbConnection _connection;
        public IDbConnection CreateConnection()
        {
            if (_connection == null)
            {
                _connection = InitConnectionString();
                _connection.Open();
                return _connection;
            }
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            return _connection;
        }
        public void BeginTransaction()
        {
            _ = CreateConnection();
            _transaction = _connection.BeginTransaction();

        }
        public void CommitTransaction()
        {
            _transaction?.Commit();
            CloseConnection();
        }
        public void RollBackTransaction()
        {
            _transaction?.Rollback();
            CloseConnection();
        }

        public T OpenConnection<T>(string commandText, CommandType commandType, object param = null, Func<IDbCommand, T> func = null)
        {
            _ = CreateConnection();
            using var cmd = CreateCommand(commandText, commandType, param);
            try
            {
                var result = func.Invoke(cmd);
                return result;
            }
            catch (Exception ex)
            {
                dbLogger?.Invoke(ex, cmd, "", "", nameof(SQLFactory));
                throw ex;
            }
            finally
            {
                cmd.Dispose();
                CloseConnection();
            }
        }
        public T StoreExecuteScalar<T>(string commandText, object parameters) => ExecuteScalar<T>(commandText, CommandType.StoredProcedure, parameters);
        public int StoreExecuteNonQuery(string commandText, object parameters) => ExecuteNonQuery(commandText, CommandType.StoredProcedure, parameters);
        public T StoreExecuteToSingle<T>(string commandText, object parameters) => ExecuteToSingle<T>(commandText, CommandType.StoredProcedure, parameters);
        public List<T> StoreExecuteToList<T>(string commandText, object parameters) => ExecuteToList<T>(commandText, CommandType.StoredProcedure, parameters);
        public IEnumerable<T> StoreExecuteToEnumerable<T>(string commandText, object parameters) => ExecuteToEnumerable<T>(commandText, CommandType.StoredProcedure, parameters);
        public IEnumerable<IDictionary<string, object>> StoreSelectDictionary(string commandText, object parameters) => SelectDictionary(commandText, CommandType.StoredProcedure, parameters);
        public T ExecuteScalar<T>(string commandText, CommandType commandType, object param)
        {
            return OpenConnection(commandText, commandType, param, (dbCommand) =>
            {
                var obj = dbCommand.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? default(T) : (T)obj;
            });
        }
        public int ExecuteNonQuery(string commandText, CommandType commandType, object param = null)
        {
            return OpenConnection(commandText, commandType, param, (dbCommand) =>
            {
                return dbCommand.ExecuteNonQuery();
            });
        }
        public T ExecuteToSingle<T>(string commandText, CommandType commandType, object param = null)
        {
            return OpenConnection<T>(commandText, commandType, param, (dbCommand) => MappingReadToSingle<T>(dbCommand));
        }
        public List<T> ExecuteToList<T>(string commandText, CommandType commandType, object param = null)
        {
            return OpenConnection(commandText, commandType, param, (dbCommand) => MappingReaderToEnumerable<T>(dbCommand).ToList());
        }
        public IEnumerable<IDictionary<string, object>> SelectDictionary(string commandText, CommandType commandType, object param = null)
        {
            using var connection = CreateConnection();
            using var cmd = CreateCommand(commandText, commandType, param);

            using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            if (reader.FieldCount == 0)
                yield break;

            var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            foreach (IDataRecord record in reader as IEnumerable)
                yield return names.ToDictionary(n => n, n => record[n]);

        }
        public IEnumerable<T> ExecuteToEnumerable<T>(string commandText, CommandType commandType, object parameters = null)
        {
            return OpenConnection(commandText, commandType, parameters, MappingReaderToEnumerable<T>);
        }

        protected IDbCommand CreateCommand(string commandText, CommandType commandType = CommandType.StoredProcedure, object objParameters = null)
        {
            var dbCommand = InitCommand(commandText, commandType, objParameters);
            var parameters = MappingPrams(objParameters);

            foreach (var dbDataParameter in parameters)
                dbCommand.Parameters.Add(dbDataParameter);

            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            dbCommand.Connection = _connection;
            dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        private static IEnumerable<T> MappingReaderToEnumerable<T>(IDbCommand dbCommand)
        {
            using var dataReader = dbCommand.ExecuteReader();
            if (dataReader.FieldCount == 0)
                yield break;
            var builder = DynamicBuilder<T>.CreateBuilder(dataReader);
            while (dataReader.Read())
                yield return builder.Build(dataReader);
        }
        private static T MappingReadToSingle<T>(IDbCommand dbCommand)
        {
            using var dataReader = dbCommand.ExecuteReader();
            if (dataReader.FieldCount == 0)
                return default(T);

            var builder = DynamicBuilder<T>.CreateBuilder(dataReader);

            if (!dataReader.Read())
                return default(T);

            var record = builder.Build(dataReader);
            if (dataReader.Read())
                throw new Exception("The result is not single row.");
            return record;
        }

        private void CloseConnection()
        {
            if (_transaction == null && _connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }
        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();

                _connection.Dispose();
                _connection = null;
            }
        }

    }
}
