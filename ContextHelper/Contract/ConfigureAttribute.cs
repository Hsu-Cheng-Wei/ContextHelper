using System;

namespace ContextHelper.Contract
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class ConfigureAttribute : Attribute
    {
        public object[] Parameters { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
    }
}
