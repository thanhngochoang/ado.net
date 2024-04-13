using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Tony.SampeAdo.Infastructure.Extentions

{
    /// <summary>
    /// Use for an alternative param name other than the propery name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryParamNameAttribute : Attribute
    {
        public string Name { get; set; }
        public QueryParamNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Ignore this property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryParamIgnoreAttribute : Attribute
    {
    }

    public static class SqlParameterExtensions
    {
        private class QueryParamInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }



        public static object[] ToSqlParamsArray(this object obj, SqlParameter[] additionalParams = null)
        {
            var result = obj.ToSqlParamsList(additionalParams);
            return result.ToArray<object>();

        }

        public static IEnumerable<SqlParameter> ToSqlParamsList(this object obj, SqlParameter[] additionalParams = null)
        {
            return obj.GetParamList().Concat(additionalParams ?? Enumerable.Empty<SqlParameter>());
        }
        private static IEnumerable<SqlParameter> GetParamList(this object obj)
        {

            var props = (
                from p in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public)
                let nameAttr = p.GetCustomAttributes(typeof(QueryParamNameAttribute), true)
                let ignoreAttr = p.GetCustomAttributes(typeof(QueryParamIgnoreAttribute), true)
                select new { Property = p, Names = nameAttr, Ignores = ignoreAttr }).ToList();


            foreach (var p in props)
            {
                if (p.Ignores != null && p.Ignores.Length > 0)
                    yield break;

                var name = p.Names.FirstOrDefault() as QueryParamNameAttribute;
                var pinfo = new QueryParamInfo();

                if (name != null && !string.IsNullOrWhiteSpace(name.Name))
                    pinfo.Name = name.Name.Replace("@", "");
                else
                    pinfo.Name = p.Property.Name.Replace("@", "");

                pinfo.Value = p.Property.GetValue(obj) ?? DBNull.Value;
                var sqlParam = new SqlParameter(pinfo.Name, TypeConverter.ToSqlDbType(p.Property.PropertyType))
                {
                    Value = pinfo.Value
                };
                yield return sqlParam;
            }
        }
    }


    //Convert .Net Type to SqlDbType or DbType and vise versa
    //This class can be useful when you make conversion between types .The class supports conversion between .Net Type , SqlDbType and DbType .
    //https://gist.github.com/abrahamjp/858392

    /// <summary>
    /// Convert a base data type to another base data type
    /// </summary>
    public sealed class TypeConverter
    {

        private struct DbTypeMapEntry
        {
            public Type Type;
            public DbType DbType;
            public SqlDbType SqlDbType;
            public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
            {
                Type = type;
                DbType = dbType;
                SqlDbType = sqlDbType;
            }

        };

        private static readonly Lazy<Dictionary<Type, DbTypeMapEntry>> _typeMapList = new Lazy<Dictionary<Type, DbTypeMapEntry>>(() =>
        {
            return _mapperSource.ToDictionary(x => x.Type);
        });
        private static readonly Lazy<Dictionary<DbType, DbTypeMapEntry>> _dbTypeMaplist = new Lazy<Dictionary<DbType, DbTypeMapEntry>>(() =>
        {
            return _mapperSource.ToDictionary(x => x.DbType);
        });

        private static readonly Lazy<Dictionary<SqlDbType, DbTypeMapEntry>> _sqlTypeMaplist = new Lazy<Dictionary<SqlDbType, DbTypeMapEntry>>(() =>
        {
            return _mapperSource.ToDictionary(x => x.SqlDbType);
        });

        #region Constructors
        private static IEnumerable<DbTypeMapEntry> _mapperSource
        {
            get
            {
                yield return new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit);
                yield return new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt);
                yield return new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image);
                yield return new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime);
                yield return new DbTypeMapEntry(typeof(decimal), DbType.Decimal, SqlDbType.Decimal);
                yield return new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float);
                yield return new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier);
                yield return new DbTypeMapEntry(typeof(short), DbType.Int16, SqlDbType.SmallInt);
                yield return new DbTypeMapEntry(typeof(int), DbType.Int32, SqlDbType.Int);
                yield return new DbTypeMapEntry(typeof(long), DbType.Int64, SqlDbType.BigInt);
                yield return new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
                yield return new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar);

            }
        }




        #endregion

        #region Methods

        /// <summary>
        /// Convert db type to .Net data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type ToNetType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert TSQL type to .Net data type
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static Type ToNetType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert .Net type to Db type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType ToDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.DbType;
        }

        /// <summary>
        /// Convert TSQL data type to DbType
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static DbType ToDbType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.DbType;
        }

        /// <summary>
        /// Convert .Net type to TSQL data type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.SqlDbType;
        }

        /// <summary>
        /// Convert DbType type to TSQL data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            if (_typeMapList.Value.TryGetValue(Nullable.GetUnderlyingType(type) ?? type, out var entry))
            {
                return entry;
            }
            throw new ApplicationException("Referenced an unsupported Type " + type.ToString());
        }

        private static DbTypeMapEntry Find(DbType dbType)
        {

            if (_dbTypeMaplist.Value.TryGetValue(dbType, out var entry))
            {
                return entry;
            }
            throw new ApplicationException("Referenced an unsupported DbType " + dbType.ToString());
        }

        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            if (_sqlTypeMaplist.Value.TryGetValue(sqlDbType, out var entry))
            {
                return entry;
            }
            throw new ApplicationException("Referenced an unsupported SqlDbType");
        }

        #endregion
    }
}
