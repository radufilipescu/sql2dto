using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class KeyPropsAttribute : Attribute
    {
        public string[] KeyPropNames { get; private set; }
        public KeyPropsAttribute(params string[] keyPropNames)
        {
            KeyPropNames = KeyPropNames;
        }
    }
}
