using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropMapAttribute : Attribute
    {
        public string ColumnName { get; set; }
        public int? ColumnOrdinal { get; set; }
    }
}
