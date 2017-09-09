using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ColumnsPrefixAttribute : Attribute
    {
        public string Value { get; private set; }

        public ColumnsPrefixAttribute(string value)
        {
            Value = value;
        }
    }
}
