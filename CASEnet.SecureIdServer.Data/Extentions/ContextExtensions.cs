using Microsoft.AspNetCore.Http;

namespace CASEnet.SecureIdServer.Data.Extentions
{
    public static class ContextExtensions
    {
        public static string GetIPAddress(this HttpContext context)
        {

            if (context.Request.Headers.ContainsKey("MS_HttpContext"))
            {
                return context.Request.Headers["MS_HttpContext"];
            }
            return context.Connection.RemoteIpAddress.ToString();
           
        }
    }
}
