using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace sql2dto.Core
{
    public class PropMapConfig
    {
        public readonly object DefaultValue;
        public readonly PropertyInfo Info;

        public PropMapConfig(PropertyInfo info, object defaultValue)
        {
            this.Info = info;
            this.DefaultValue = defaultValue;
        }

        public PropMapConfig Clone()
        {
            var clone = new PropMapConfig(this.Info, this.DefaultValue);
            clone.ColumnName = this.ColumnName;
            clone.ColumnOrdinal = this.ColumnOrdinal;
            clone.Converter = this.Converter;
            return clone;
        }

        public string ColumnName { get; set; }
        public int? ColumnOrdinal { get; set; }
        public Func<object, object> Converter { get; set; }
    }
}
