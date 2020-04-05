using CASEnet.SecureIdServer.Data;
using Microsoft.Extensions.Options;
using System;
using System.Data;
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

        public Guid RegisterDevice(string phone, string confirmationcode)
        {
            var installId = ExecuteScalar<Guid>("[dbo].[RegisterDevice]", CommandType.StoredProcedure, new { phone, confirmationcode });
            return installId;
        }

        public int ConfirmDeviceRegistration(Guid installationid, string confirmationcode)
        {
            return ExecuteScalar<int>("[dbo].[ConfirmDeviceRegistration]", CommandType.StoredProcedure, new { installationid, confirmationcode });
        }

        public string RequestDeviceCode(Guid installationid)
        {
            var code = ExecuteScalar<string>("[dbo].[RequestDeviceCode]", CommandType.StoredProcedure, new { installationid });
            if (code == null) throw new ArgumentNullException();
            return code;
        }

        public void UpdateDeviceCode(string phone, string code)
        {
            ExecuteScalar<Guid>("[dbo].[UpdateDeviceCode]", CommandType.StoredProcedure, new { phone, code });
        }

    }
}
