using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tony.SampeAdo.Infastructure.Extentions;

namespace Tony.SampeAdo.Infastructure
{
    public abstract class BaseDb : IDisposable
    {

        #region Abstract methods

        protected abstract IDbConnection CreateConnection();

        protected abstract IDbCommand CreateCommand();

        #endregion
        #region Connection and transaction

        private IDbConnection _connection;

        private IDbTransaction _transaction;

        private T OpenConnection<T>(Func<T> func)
        {
            if (_connection == null)
            {
                _connection = CreateConnection();

                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            var result = func.Invoke();

            CloseConnection();

            return result;
        }

        private void CloseConnection()
        {
            if (_transaction == null && _connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public void BeginTransaction()
        {
            OpenConnection(() =>
            {
                _transaction = _connection.BeginTransaction();
                return _transaction;
            });
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            OpenConnection(() =>
            {
                _transaction = _connection.BeginTransaction(isolationLevel);
                return _transaction;
            });
        }

        public void RollbackTransaction()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;

            CloseConnection();
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;

            CloseConnection();
        }

        #endregion

        #region Private functions

        private IDbCommand CreateCommand(string commandText, CommandType commandType)
        {
            var dbCommand = CreateCommand();

            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;

            dbCommand.Connection = _connection;
            dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        private IDbCommand CreateCommand(string commandText, CommandType commandType, params IDbDataParameter[] parameters)
        {
            FixParameterNullValue(parameters);

            var dbCommand = CreateCommand();
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            foreach (var dbDataParameter in parameters)
            {
                dbCommand.Parameters.Add(dbDataParameter);
            }
            dbCommand.Connection = _connection;
            dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        private IDbCommand CreateCommand(string commandText, CommandType commandType, object objParameters)
        {
            var dbCommand = CreateCommand();
            var parameters = objParameters.ToSqlParamsList();
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            foreach (var dbDataParameter in parameters)
            {
                dbCommand.Parameters.Add(dbDataParameter);
            }
            dbCommand.Connection = _connection;
            dbCommand.Transaction = _transaction;

            return dbCommand;
        }

        private static void FixParameterNullValue(IEnumerable<IDbDataParameter> parameters)
        {
            var query = parameters.Where(par => par.Value == null);
            foreach (var par in query)
            {
                par.Value = DBNull.Value;
            }
        }

        private static List<T> MappingReaderToList<T>(IDbCommand dbCommand)
        {
            using (var dataReader = dbCommand.ExecuteReader())
            {
                if (dataReader.FieldCount == 0)
                {
                    return null;
                }
                var recordList = new List<T>();

                var builder = DynamicBuilder<T>.CreateBuilder(dataReader);

                while (dataReader.Read())
                {
                    var record = builder.Build(dataReader);
                    recordList.Add(record);
                }

                return recordList;
            }
        }

        private static T MappingReadToSingle<T>(IDbCommand dbCommand)
        {
            using (var dataReader = dbCommand.ExecuteReader())
            {
                if (dataReader.FieldCount == 0)
                {
                    return default;
                }

                var builder = DynamicBuilder<T>.CreateBuilder(dataReader);

                if (!dataReader.Read())
                {
                    return default;
                }
                var record = builder.Build(dataReader);

                if (dataReader.Read())
                {
                    throw new Exception("The result is not single row.");
                }
                return record;
            }
        }

        private T ExecuteScalar<T>(IDbCommand dbCommand)
        {
            var obj = dbCommand.ExecuteScalar();
            return obj == null || obj == DBNull.Value ? default : (T)obj;
        }

        private int ExecuteNonQuery(IDbCommand dbCommand)
        {
            return dbCommand.ExecuteNonQuery();
        }

        #endregion

        #region Execute to list

        public List<T> ExecuteToList<T>(string commandText, CommandType commandType)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType))
                {
                    return MappingReaderToList<T>(dbCommand);
                }
            });
        }

        public List<T> ExecuteToList<T>(string commandText, CommandType commandType, params IDbDataParameter[] parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return MappingReaderToList<T>(dbCommand);
                }
            });
        }

        public List<T> ExecuteToList<T>(string commandText, CommandType commandType, object parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return MappingReaderToList<T>(dbCommand);
                }
            });
        }

        #endregion

        #region Execute to single

        public T ExecuteToSingle<T>(string commandText, CommandType commandType)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType))
                {
                    return MappingReadToSingle<T>(dbCommand);
                }
            });
        }

        public T ExecuteToSingle<T>(string commandText, CommandType commandType, params IDbDataParameter[] parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return MappingReadToSingle<T>(dbCommand);
                }
            });
        }

        public T ExecuteToSingle<T>(string commandText, CommandType commandType, object parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return MappingReadToSingle<T>(dbCommand);
                }
            });
        }

        #endregion

        #region Execute scalar

        public T ExecuteScalar<T>(string commandText, CommandType commandType)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType))
                {
                    return ExecuteScalar<T>(dbCommand);
                }
            });
        }

        public T ExecuteScalar<T>(string commandText, CommandType commandType, params IDbDataParameter[] parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return ExecuteScalar<T>(dbCommand);
                }
            });
        }

        public T ExecuteScalar<T>(string commandText, CommandType commandType, object parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return ExecuteScalar<T>(dbCommand);
                }
            });
        }
        #endregion

        #region Execute non query

        public int ExecuteNonQuery(string commandText, CommandType commandType)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType))
                {
                    return ExecuteNonQuery(dbCommand);
                }
            });
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType, params IDbDataParameter[] parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return ExecuteNonQuery(dbCommand);
                }
            });
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType, object parameters)
        {
            return OpenConnection(() =>
            {
                using (var dbCommand = CreateCommand(commandText, commandType, parameters))
                {
                    return ExecuteNonQuery(dbCommand);
                }
            });
        }

        #endregion

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
