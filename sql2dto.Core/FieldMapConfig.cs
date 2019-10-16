using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace sql2dto.Core
{
    public class FieldMapConfig
    {
        public readonly object DefaultValue;
        public readonly FieldInfo Info;

        public FieldMapConfig(FieldInfo info, object defaultValue)
        {
            this.Info = info;
            this.DefaultValue = defaultValue;

            Type stringType = typeof(string);

            Type innerPropType = null;
            string innerPropTypeName = null;
            string getterName = "Get";
            if (this.Info.FieldType.IsGenericType
                && this.Info.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                innerPropType = Nullable.GetUnderlyingType(this.Info.FieldType);
                innerPropTypeName = innerPropType.Name;
                getterName += "Nullable" + innerPropTypeName;
            }
            else if (this.Info.FieldType == stringType)
            {
                innerPropType = stringType;
                innerPropTypeName = stringType.Name;
                getterName += "Nullable" + innerPropTypeName;
            }
            else
            {
                innerPropType = this.Info.FieldType;
                innerPropTypeName = innerPropType.Name;
                getterName += innerPropTypeName;
            }
            this.InnerPropType = innerPropType;
            this.InnerPropTypeName = innerPropTypeName;
            this.ReadHelperGetterMethodName = getterName;
            this.ReadHelperGetterMethodInfo = ReadHelper.GetGetterOrdinalMethodInfo(this.ReadHelperGetterMethodName);
        }

        public FieldMapConfig Clone()
        {
            var clone = new FieldMapConfig(this.Info, this.DefaultValue);
            clone.DeclarationOrder = this.DeclarationOrder;
            clone.ColumnName = this.ColumnName;
            clone.ColumnOrdinal = this.ColumnOrdinal;
            clone.Converter = this.Converter;
            clone.IsKey = this.IsKey;
            clone.IsNullableKey = this.IsNullableKey;
            return clone;
        }

        public int DeclarationOrder { get; internal set; }
        public string ColumnName { get; internal set; }
        public int? ColumnOrdinal { get; internal set; }
        public bool IsKey { get; internal set; }
        public bool IsNullableKey { get; internal set; }
        public Func<object, object> Converter { get; internal set; }

        internal readonly Type InnerPropType;
        internal readonly string InnerPropTypeName;
        internal readonly string ReadHelperGetterMethodName;
        internal readonly MethodInfo ReadHelperGetterMethodInfo;
    }
}
