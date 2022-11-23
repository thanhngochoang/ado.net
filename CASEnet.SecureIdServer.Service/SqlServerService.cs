using CASEnet.SecureIdServer.Data.Extentions;
using CASEnet.SecureIdServer.Data.Interface;
using CASEnet.SecureIdServer.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CASEnet.SecureIdServer.Data
{
    public abstract class SqlServerService<T> : SQLFactory
    {
        private readonly AppSettings _settings;
        private readonly ILoggerServices _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected delegate SqlParameter Param(SqlParameter param);

        protected delegate IEnumerable<SqlParameter> ParamExtend();

        protected Param? UpdateParameter;
        protected ParamExtend? ExtendParameter;

        public SqlServerService(IOptions<AppSettings> options, ILoggerServices logger, IHttpContextAccessor httpContextAccessor)
        {
            _settings = options.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            dbLogger = DbLogException;
        }

        protected override IDbCommand InitCommand(string commandText, CommandType commandType, object objParameters)
        {
            return new SqlCommand();
        }

        protected string CreatedByIP => _httpContextAccessor.HttpContext.GetIPAddress();
        protected string RequestPath => _httpContextAccessor.HttpContext.Request.Path;

        protected override IDbConnection InitConnectionString()
        {
            return new SqlConnection(_settings.ConnectionString);
        }

        //Customize logger
        public void DbLogException(Exception ex, IDbCommand cmd, string createdByIP, string requestUrl, string created_By = "Execute DB")
        {
            _logger.DbLogException(ex, cmd, CreatedByIP, RequestPath, typeof(T).Name);
        }

        protected IEnumerable<SqlParameter> ObjectToPrams(object objParameters, IEnumerable<SqlParameter>? extends)
        {
            var prams = TypeConvertor.ToSqlParamsList(
                (p) => new SqlParameter(p.Name, TypeConvertor.GetDbType(p.Property.PropertyType)) { Value = p.Value }
                , objParameters, extends);
            return prams;
        }

        protected override IEnumerable<object> MappingPrams(object objParameters)
        {
            if (objParameters == null)
                yield break;
            var extendParam = ExtendParameter?.Invoke() ?? null;
            var prams = ObjectToPrams(objParameters, extendParam);

            foreach (var item in prams)
            {
                UpdateParameter?.Invoke(item);
                yield return item;
            }
        }
    }
}