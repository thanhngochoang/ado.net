using CASEnet.SecureIdServer.Data.Interface;
using CASEnet.SecureIdServer.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace CASEnet.SecureIdServer.Data
{
    public sealed class DeviceDbSql : SqlServerService<DeviceDbSql>
    {
        public  DeviceDbSql(IOptions<AppSettings> options, ILoggerServices logger, IHttpContextAccessor httpContextAccessor) : base(options, logger, httpContextAccessor)
        {
        }

        public List<int> GetFromDB()
        {
            return ExecuteToList<int>("spStore", System.Data.CommandType.StoredProcedure, new { param1 = 1 });
        }
    }

}
