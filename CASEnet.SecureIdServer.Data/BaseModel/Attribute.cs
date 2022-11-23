using System;

namespace CASEnet.SecureIdServer.Data.BaseModel
{
    /// <summary>
    /// Use for an alternative param name other than the propery name
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public sealed class QueryParamNameAttribute : Attribute
    {
        public string Name { get; set; }
        public QueryParamNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Ignore this property
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public sealed class QueryParamIgnoreAttribute : Attribute
    {
    }
}
