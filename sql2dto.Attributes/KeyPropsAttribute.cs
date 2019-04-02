using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.type.getproperties?view=netframework-4.7.2
    // The Type.GetProperties method does not return properties in a particular order, such as alphabetical or declaration order.
    // Your code must not depend on the order in which properties are returned, because that order varies.

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class KeyPropsAttribute : Attribute
    {
        public string[] KeyPropNames { get; private set; }
        public KeyPropsAttribute(params string[] keyPropNames)
        {
            KeyPropNames = keyPropNames;
        }
    }
}
