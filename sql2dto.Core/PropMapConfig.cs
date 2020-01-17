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

        public PropMapConfig(PropertyInfo info)
        {
            Type stringType = typeof(string);

            this.Info = info;
            this.DefaultValue = info.PropertyType == stringType ? null : Activator.CreateInstance(info.PropertyType);

            Type innerPropType;
            string innerPropTypeName;
            string getterName = "Get";
            if (this.Info.PropertyType.IsGenericType
                && this.Info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                innerPropType = Nullable.GetUnderlyingType(this.Info.PropertyType);
                innerPropTypeName = innerPropType.Name;
                getterName += "Nullable" + innerPropTypeName;
            }
            else if (this.Info.PropertyType == stringType)
            {
                innerPropType = stringType;
                innerPropTypeName = stringType.Name;
                getterName += "Nullable" + innerPropTypeName;
            }
            else
            {
                innerPropType = this.Info.PropertyType;
                innerPropTypeName = innerPropType.Name;
                getterName += innerPropTypeName;
            }
            this.InnerPropType = innerPropType;
            this.InnerPropTypeName = innerPropTypeName;
            this.ReadHelperGetterMethodName = getterName;
            this.ReadHelperGetterMethodInfo = ReadHelper.GetGetterOrdinalMethodInfo(this.ReadHelperGetterMethodName);
        }

        public PropMapConfig Clone()
        {
            var clone = new PropMapConfig(this.Info);
            clone.DeclarationOrder = this.DeclarationOrder;
            clone.ColumnName = this.ColumnName;
            clone.ColumnOrdinal = this.ColumnOrdinal;
            clone.Converter = this.Converter;
            clone.IsKey = this.IsKey;
            clone.IsNullableKey = this.IsNullableKey;
            clone.Ignored = this.Ignored;
            return clone;
        }

        public int DeclarationOrder { get; internal set; }
        public string ColumnName { get; internal set; }
        public int? ColumnOrdinal { get; internal set; }
        public bool IsKey { get; internal set; }
        public bool IsNullableKey { get; internal set; }
        public Func<object, object> Converter { get; internal set; }
        public bool Ignored { get; internal set; }

        internal readonly Type InnerPropType;
        internal readonly string InnerPropTypeName;
        internal readonly string ReadHelperGetterMethodName;
        internal readonly MethodInfo ReadHelperGetterMethodInfo;
    }
}
