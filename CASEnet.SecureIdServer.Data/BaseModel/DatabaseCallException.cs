using System;
using System.Data;

namespace CASEnet.SecureIdServer.Data.Model
{
    public class DatabaseCallException : Exception
    {
        public string StoredProcedureName { get; set; }
        public string StoredProcedureParameters { get; set; }
        public DatabaseCallException(string msg, IDbCommand cmd, Exception inner = null) : base(msg, inner)
        {
            StoredProcedureName = cmd.CommandText;
            StoredProcedureParameters = string.Join(Environment.NewLine, cmd.Parameters);
        }
    }
}
