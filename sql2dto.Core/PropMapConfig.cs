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

            string getterName = "Get";
            if (this.Info.PropertyType.IsGenericType
                && this.Info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                getterName += "Nullable" + Nullable.GetUnderlyingType(this.Info.PropertyType).Name;
            }
            else if (this.Info.PropertyType == typeof(string))
            {
                getterName += "NullableString";
            }
            else
            {
                getterName += this.Info.PropertyType.Name;
            }
            this.ReadHelperGetterMethodName = getterName;
            this.ReadHelperGetterMethodInfo = ReadHelper.GetGetterOrdinalMethodInfo(this.ReadHelperGetterMethodName);
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

        internal readonly string ReadHelperGetterMethodName;
        internal readonly MethodInfo ReadHelperGetterMethodInfo;
    }
}
