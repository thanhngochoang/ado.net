using CASEnet.SecureIdServer.Data.BaseModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CASEnet.SecureIdServer.Data.Extentions
{
    public static class TypeConvertor
    {
       

            private struct DbTypeMapEntry
            {
                public DbType DbType;
                public SqlDbType SqlDbType;
                public DbTypeMapEntry(DbType dbType, SqlDbType sqlDbType)
                {
                    this.DbType = dbType;
                    this.SqlDbType = sqlDbType;
                }
            };

            private static Dictionary<Type, MySqlDbType> typeMap;
            private static Dictionary<Type, DbTypeMapEntry> _DbTypeList;

            #region Constructors
            static TypeConvertor()
            {
                typeMap = new Dictionary<Type, MySqlDbType>();
                _DbTypeList = new Dictionary<Type, DbTypeMapEntry>();

                typeMap[typeof(string)] = MySqlDbType.VarString;
                typeMap[typeof(char[])] = MySqlDbType.VarChar;
                typeMap[typeof(byte)] = MySqlDbType.Byte;
                typeMap[typeof(short)] = MySqlDbType.Int16;
                typeMap[typeof(int)] = MySqlDbType.Int32;
                typeMap[typeof(long)] = MySqlDbType.Int64;
                typeMap[typeof(byte[])] = MySqlDbType.Blob;
                typeMap[typeof(bool)] = MySqlDbType.Bit;
                typeMap[typeof(DateTime)] = MySqlDbType.DateTime;
                typeMap[typeof(DateTimeOffset)] = MySqlDbType.Timestamp;
                typeMap[typeof(decimal)] = MySqlDbType.Decimal;
                typeMap[typeof(float)] = MySqlDbType.Float;
                typeMap[typeof(double)] = MySqlDbType.Double;
                typeMap[typeof(TimeSpan)] = MySqlDbType.Time;
                typeMap[typeof(Guid)] = MySqlDbType.Guid;

                _DbTypeList.Add(typeof(bool), new DbTypeMapEntry(DbType.Boolean, SqlDbType.Bit));
                _DbTypeList.Add(typeof(byte), new DbTypeMapEntry(DbType.Double, SqlDbType.TinyInt));
                _DbTypeList.Add(typeof(byte[]), new DbTypeMapEntry(DbType.Binary, SqlDbType.Image));
                _DbTypeList.Add(typeof(DateTime), new DbTypeMapEntry(DbType.DateTime, SqlDbType.DateTime));
                _DbTypeList.Add(typeof(decimal), new DbTypeMapEntry(DbType.Decimal, SqlDbType.Decimal));
                _DbTypeList.Add(typeof(double), new DbTypeMapEntry(DbType.Double, SqlDbType.Float));
                _DbTypeList.Add(typeof(Guid), new DbTypeMapEntry(DbType.Guid, SqlDbType.UniqueIdentifier));
                _DbTypeList.Add(typeof(Int16), new DbTypeMapEntry(DbType.Int16, SqlDbType.SmallInt));
                _DbTypeList.Add(typeof(Int32), new DbTypeMapEntry(DbType.Int32, SqlDbType.Int));
                _DbTypeList.Add(typeof(Int64), new DbTypeMapEntry(DbType.Int64, SqlDbType.BigInt));
                _DbTypeList.Add(typeof(object), new DbTypeMapEntry(DbType.Object, SqlDbType.Variant));
                _DbTypeList.Add(typeof(string), new DbTypeMapEntry(DbType.String, SqlDbType.VarChar));
            }
            #endregion

            #region Methods

            public static MySqlDbType GetDbType(Type giveType)
            {
                giveType = Nullable.GetUnderlyingType(giveType) ?? giveType;
                if (typeMap.ContainsKey(giveType))
                    return typeMap[giveType];

                throw new ArgumentException($"{giveType.FullName} is not a supported .NET class");
            }

            public static SqlDbType GetSQLDbType(Type giveType)
            {
                giveType = Nullable.GetUnderlyingType(giveType) ?? giveType;
                if (_DbTypeList.ContainsKey(giveType))
                    return _DbTypeList[giveType].SqlDbType;

                throw new ArgumentException($"{giveType.FullName} is not a supported .NET class");
            }
            public static DbType ToDbType(Type giveType)
            {
                giveType = Nullable.GetUnderlyingType(giveType) ?? giveType;
                if (_DbTypeList.ContainsKey(giveType))
                    return _DbTypeList[giveType].DbType;

                throw new ArgumentException($"{giveType.FullName} is not a supported .NET class");
            }

            public static MySqlDbType GetDbType<T>()
            {
                return GetDbType(typeof(T));
            }

            public static IEnumerable<T> ToSqlParamsList<T>(Func<QueryParamInfo, T> func, object obj = null, IEnumerable<T> additionalParams = null)
            {
                if (obj == null && additionalParams?.Count() == 0)
                    yield break;
                var props = (
                    from p in obj.GetType().GetProperties()
                    let nameAttr = p.GetCustomAttributes(typeof(QueryParamNameAttribute), true)
                    let ignoreAttr = p.GetCustomAttributes(typeof(QueryParamIgnoreAttribute), true)
                    select new { Property = p, Names = nameAttr, Ignores = ignoreAttr }).ToList();

                for (int i = 0; i < props.Count; i++)
                {
                    var p = props[i];
                    if (p.Ignores != null && p.Ignores.Length > 0)
                        continue;

                    var name = p.Names.FirstOrDefault() as QueryParamNameAttribute;
                    var pinfo = new QueryParamInfo();

                    if (name != null && !string.IsNullOrWhiteSpace(name.Name))
                        pinfo.Name = name.Name.Replace("@", "");
                    else
                        pinfo.Name = p.Property.Name.Replace("@", "");

                    pinfo.Value = p.Property.GetValue(obj) ?? DBNull.Value;
                    pinfo.Property = p.Property;

                    yield return func.Invoke(pinfo);
                }


                if (additionalParams?.Count() > 0)
                    foreach (var item in additionalParams)
                        yield return item;


            }
            public class QueryParamInfo
            {
                public string Name { get; set; }
                public object Value { get; set; }
                public PropertyInfo Property { get; set; }
            }
            #endregion
        }
}
