using CASEnet.SecureIdServer.Data;
using CASEnet.SecureIdServer.Data.Extentions;
using CASEnet.SecureIdServer.Data.Interface;
using CASEnet.SecureIdServer.Data.Model;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CASEnet.SecureIdServer.Service.Implement
{
    public class LoggerServices : SQLFactory, ILoggerServices
    {
        public LoggerServices(IOptions<AppSettings> options)
        {
            _settings = options.Value;
            dbLogger = DbLogException;
        }
        private readonly AppSettings _settings;

        protected override IDbConnection InitConnectionString()
        {
            return new SqlConnection(_settings.ConnectionString);
        }

        protected override IEnumerable<object> MappingPrams(object objParameters)
        {
            return TypeConvertor.ToSqlParamsList((p) => new MySqlParameter(p.Name, TypeConvertor.GetDbType(p.Property.PropertyType)) { Value = p.Value } , objParameters);
        }

        protected override IDbCommand InitCommand(string commandText, CommandType commandType, object objParameters)
        {
            return new SqlCommand();
        }
        public static IEnumerable<string> GetParamsLog(IDbCommand cmd)
        {
            var enumerator = cmd.Parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                MySqlParameter mySqlParameter = (MySqlParameter)enumerator.Current;
                yield return $"{mySqlParameter?.ParameterName}:{mySqlParameter?.Value}";
            }
            if (enumerator is IDisposable e)
                e.Dispose();
        }

        public void DbLogException(Exception ex, IDbCommand cmd,  string createdByIP, string requestUrl, string created_By = "Execute DB")
        {
            var storedProcedureName = cmd.CommandText;
            var storedProcedureParameters = string.Join(Environment.NewLine, GetParamsLog(cmd));

            dbExecuteLog(ex.GetType().ToString(),
                _settings.AppName,
                ex.InnerException != null ? ex.InnerException.ToString() : "",
                ex.Message,
                ex.StackTrace,
                requestUrl,
                storedProcedureName,
                storedProcedureParameters,
                created_By,
                createdByIP);
        }
        public void DbLogException(DatabaseCallException dbex, string createdByIP, string requestUrl)
        {
            var storedProcedureName = dbex.StoredProcedureName;
            var storedProcedureParameters = dbex.StoredProcedureParameters;

            dbExecuteLog(dbex.GetType().ToString(),
                _settings.AppName,
                dbex.InnerException != null ? dbex.InnerException.ToString() : "",
                dbex.Message ?? "",
                dbex.StackTrace,
                requestUrl,
                storedProcedureName,
                storedProcedureParameters,
                "Execute DB",
                createdByIP);
        }

        public void LogException(Exception ex, string createdBy, string requestUrl, string createdByIP)
        {
            try
            {
                LogException(ex, ex.Message, createdBy, createdByIP, _settings.AppName, requestUrl);
            }
            catch (Exception) 
            {
                throw;
            }
        }
        public void LogException(Exception ex, string messenger, string createdBy, string requestUrl, string createdByIP)
        {
            try
            {
                LogException(ex, messenger, createdBy, createdByIP, _settings.AppName, requestUrl);
            }
            catch (Exception) 
            {
                throw;
            }
        }
        private void LogException(Exception ex, string messensger = null, string createdBy = "API", string createdByIP = "1:1:1:1", string logSource = "API", string requestUrl = "")
        {
            dbExecuteLog(ex.GetType().ToString(), logSource, ex.InnerException != null ? ex.InnerException.ToString() : "", messensger ?? ex.Message, ex.StackTrace, requestUrl, null, null, createdBy, createdByIP);
        }

        private void dbExecuteLog(string p_ClassName, string p_LogSource, string p_InnerException, string p_Message, string p_StackTrace, string p_RequestUrl, string p_StoredProcedureName, string storedProcedureParameters, string p_CreatedBy, string p_CreatedByIP)
        {
            StoreExecuteNonQuery("Helpers_WriteExceptionLog", new { p_ClassName, p_LogSource, p_InnerException, p_Message, p_StackTrace, p_RequestUrl, p_StoredProcedureName, storedProcedureParameters, p_CreatedBy, p_CreatedByIP });
        }


        public void WriteCommunicationLog(int p_InsuranceID, string p_DeliveryMethod, string p_Recipient, string p_Subject, string p_Body, string p_Username, string p_IP, int? p_CommunicationSettingsID = 1)
        {
            StoreExecuteNonQuery("Helpers_WriteCommunicationLog", new { p_InsuranceID, p_DeliveryMethod, p_Recipient, p_Subject, p_Body, p_Username, p_CommunicationSettingsID, p_IP });
        }

        public void WriteInsuranceHistory(int p_InsuranceID, string p_CertificateNumber, string p_DisplayTextEN, string p_IP)
        {
            StoreExecuteNonQuery("Helpers_WriteInsuranceHistory", new { p_InsuranceID, p_CertificateNumber, p_DisplayTextEN, p_IP });
        }

    }
}
