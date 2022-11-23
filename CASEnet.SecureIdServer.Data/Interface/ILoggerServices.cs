using CASEnet.SecureIdServer.Data.Model;
using System;
using System.Data;

namespace CASEnet.SecureIdServer.Data.Interface
{
    public interface ILoggerServices
    {
        void LogException(Exception ex, string createdBy, string requestUrl, string createdByIP);
        void DbLogException(Exception ex, IDbCommand cmd, string createdByIP, string requestUrl, string created_By = "Execute DB");
        void DbLogException(DatabaseCallException dbex, string createdByIP, string requestUrl);
        void WriteCommunicationLog(int p_InsuranceID, string p_DeliveryMethod, string p_Recipient, string p_Subject, string p_Body, string p_Username, string p_IP, int? p_CommunicationSettingsID = 1);
        void WriteInsuranceHistory(int p_InsuranceID, string p_CertificateNumber, string p_DisplayTextEN, string p_IP);
    }
}
