using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnorePropAttribute : Attribute
    {
    }
}
