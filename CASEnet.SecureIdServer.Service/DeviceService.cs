using CASEnet.SecureIdServer.Data;
using CASEnet.SecureIdServer.Data.Interface;
using CASEnet.SecureIdServer.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Data;

namespace CASEnet.SecureIdServer.Service
{
    public interface IDeviceService
    {
        Guid RegisterDevice(string phone, string confirmCode);
        int ConfirmDeviceRegistration(Guid installationId, string confirmCode);
        string RequestDeviceCode(Guid installationId);
        void UpdateDeviceCode(string phone, string code);
    }
    public class DeviceService : SqlServerService<DeviceService>, IDeviceService
    {
        public DeviceService(IOptions<AppSettings> options, ILoggerServices logger, IHttpContextAccessor httpContextAccessor) : base(options, logger, httpContextAccessor)
        {
        }

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
