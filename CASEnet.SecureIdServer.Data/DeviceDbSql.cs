using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CASEnet.SecureIdServer.Data
{
    public class DeviceDbSql : BaseDb
    {

        private readonly Settings _settings;
        public DeviceDbSql(IOptions<Settings> options): base()
        {
            _settings = options.Value;
        }

        protected override IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection(_settings.ConnectionString);
        }

        protected async Task<bool> SendSms(string phoneNumber, string code)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://api.clickatell.com/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var smsRequest = _settings.ServerSMSConnect.Replace("{phoneNumber}", phoneNumber, StringComparison.OrdinalIgnoreCase)
                                                          .Replace("{mesenger}", code, StringComparison.OrdinalIgnoreCase);
                    // HTTP GET
                    HttpResponseMessage response = await client.GetAsync(smsRequest);
                    var responeCode = await response.Content.ReadAsStringAsync();
                    return !responeCode.Contains("ERR");
                }
            }
            catch (Exception)
            {
                return false;
            }
           
        }

        public class Settings
        {
            public string ConnectionString { get; set; }
            public string ServerSMSConnect { get; set; }
        }
    }

}
