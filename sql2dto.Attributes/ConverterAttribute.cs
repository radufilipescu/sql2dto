using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ConverterAttribute : Attribute
    {
        public abstract Func<object, object> Converter { get; }
    }
}
