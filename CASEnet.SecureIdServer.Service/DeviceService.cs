using CASEnet.SecureIdServer.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CASEnet.SecureIdServer.Service
{
    public interface IDeviceService
    {
        Guid RegisterDevice(string phone, string confirmCode);
        int ConfirmDeviceRegistration(Guid installationId, string confirmCode);
        string RequestDeviceCode(Guid installationId);
        void UpdateDeviceCode(string phone, string code);
    }
    public class DeviceService : DeviceDbSql, IDeviceService
    {
        public DeviceService(IOptions<Settings> options) : base(options) { }

        public Guid RegisterDevice(string phone, string confirmCode)
        {
            if (SendSms(phone, confirmCode).Result)
            {
                var installId = ExecuteScalar<Guid>("[dbo].[RegisterDevice]",
                    System.Data.CommandType.StoredProcedure,
                      new SqlParameter("@phone", phone),
                      new SqlParameter("@confirmationcode", confirmCode));
                return installId;
            }
            throw new ArgumentException("Phone number incorrect");
        }

        public int ConfirmDeviceRegistration(Guid installationid, string confirmCode)
        {
            return ExecuteScalar<int>("[dbo].[ConfirmDeviceRegistration]", System.Data.CommandType.StoredProcedure,
                  new SqlParameter("@installationid", installationid),
                  new SqlParameter("@confirmationcode", confirmCode)
                );
        }

        public string RequestDeviceCode(Guid installationId)
        {
            var code =  ExecuteScalar<string>("[dbo].[RequestDeviceCode]",
                System.Data.CommandType.StoredProcedure,
                new SqlParameter("@installationid", installationId)
                );
            if (code == null) throw new ArgumentNullException();
            return code;
        }

        public void UpdateDeviceCode(string phone, string code)
        {
            ExecuteScalar<Guid>("[dbo].[UpdateDeviceCode]", System.Data.CommandType.StoredProcedure,
                        new SqlParameter("@phone", phone),
                        new SqlParameter("@code", code));
        }

    }
}
