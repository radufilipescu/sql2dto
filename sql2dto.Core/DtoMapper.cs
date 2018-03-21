using sql2dto.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace sql2dto.Core
{
    public partial class DtoMapper<TDto> where TDto : new()
    {
        #region STATIC
        protected static Dictionary<string, PropMapConfig> _defaultPropMapConfigs;
        internal static string DefaultColumnsPrefix { get; private set; }
        internal static string[] DefaultOrderedKeyPropNames { get; private set; }

        internal static PropMapConfig GetDefaultInnerPropMapConfig(string propName)
        {
            if (_defaultPropMapConfigs.TryGetValue(propName, out PropMapConfig cfg))
            {
                return cfg;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        internal static bool TryGetDefaultInnerPropMapConfig(string propName, out PropMapConfig result)
        {
            if (_defaultPropMapConfigs.TryGetValue(propName, out PropMapConfig cfg))
            {
                result = cfg;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        protected static void InitDefault()
        {
            _defaultPropMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
            var dtoType = typeof(TDto);

            var columnsPrefixAttr = dtoType.GetTypeInfo().GetCustomAttribute<ColumnsPrefixAttribute>();
            if (columnsPrefixAttr != null)
            {
                DefaultColumnsPrefix = columnsPrefixAttr.Value;
            }

            var keyPropsAttribute = dtoType.GetCustomAttribute<KeyPropsAttribute>();
            if (keyPropsAttribute != null)
            {
                DefaultOrderedKeyPropNames = keyPropsAttribute.KeyPropNames;
            }

            foreach (var propInfo in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!propInfo.CanWrite)
                {
                    continue;
                }

                if (propInfo.GetCustomAttribute<IgnorePropAttribute>() != null)
                {
                    continue;
                }

                object defaultValue = null;

                if (!propInfo.PropertyType.GetTypeInfo().IsValueType)
                {
                    if (propInfo.PropertyType != typeof(string))
                    {
                        continue;
                    }
                }
                else
                {
                    defaultValue = Activator.CreateInstance(propInfo.PropertyType);
                }

                var propMapConfig = new PropMapConfig(propInfo, defaultValue);

                var propMapAttribute = propInfo.GetCustomAttribute<PropMapAttribute>();
                if (propMapAttribute != null)
                {
                    propMapConfig.ColumnName = propMapAttribute.ColumnName;
                    propMapConfig.ColumnOrdinal = propMapAttribute.ColumnOrdinal;
                }

                var converterAttribute = propInfo.GetCustomAttribute<ConverterAttribute>();
                if (converterAttribute != null)
                {
                    propMapConfig.Converter = converterAttribute.Converter;
                }

                _defaultPropMapConfigs.Add(propMapConfig.Info.Name, propMapConfig);
            }
        }

        static DtoMapper()
        {
            InitDefault();
        }

        private static int? ExtractOrdinal(Dictionary<string, PropMapConfig> propMapConfigs, Dictionary<string, int> columnNamesToOrdinals, string propName, string columnsPrefix = null)
        {
            if (propMapConfigs.TryGetValue(propName, out PropMapConfig config))
            {
                return ExtractOrdinal(columnNamesToOrdinals, config, columnsPrefix);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName), $"Property map config for '{propName}' could not be found on type {typeof(TDto).FullName}");
            }
        }

        public static int? ExtractDefaultOrdinal(ReadHelper helper, string propName, string columnsPrefix = null)
        {
            return ExtractOrdinal(_defaultPropMapConfigs, helper.ColumnNamesToOrdinals, propName, columnsPrefix);
        }

        private static int? ExtractOrdinal(Dictionary<string, int> columnNamesToColumnOrdinals, PropMapConfig config, string columnsPrefix = null)
        {
            if (config.ColumnOrdinal.HasValue)
            {
                return config.ColumnOrdinal.Value;
            }
            else
            {
                string colName = config.ColumnName ?? config.Info.Name;
                if (columnsPrefix != null)
                {
                    colName = columnsPrefix + colName;
                }

                if (columnNamesToColumnOrdinals.TryGetValue(colName, out int ordinal))
                {
                    return ordinal;
                }
            }
            return null;
        }

        private static Func<TDto> CreateMapFunc(ReadHelper helper, Dictionary<string, PropMapConfig> propertyMapConfigs, string columnsPrefix = null)
        {
            var readHelperConstExpr = Expression.Constant(helper, typeof(ReadHelper));

            List<MemberBinding> memberBindings = new List<MemberBinding>();
            foreach (var propMapConfig in propertyMapConfigs.Values)
            {
                int? ordinal = ExtractOrdinal(helper.ColumnNamesToOrdinals, propMapConfig, columnsPrefix);
                if (!ordinal.HasValue)
                {
                    continue;
                }

                if (propMapConfig.Converter == null)
                {
                    Expression getValueExpr = Expression.Call(readHelperConstExpr,
                        propMapConfig.ReadHelperGetterMethodInfo, new Expression[]
                            {
                                Expression.Constant(ordinal, typeof(int))
                            }
                    );

                    var memberBinding = Expression.Bind(propMapConfig.Info, getValueExpr);
                    memberBindings.Add(memberBinding);
                }
                else
                {
                    Expression getValueExpr = Expression.Call(readHelperConstExpr,
                        ReadHelper.GetValueOrDefaultMethodInfo, new Expression[]
                            {
                                Expression.Constant(ordinal, typeof(int)),
                                Expression.Constant(propMapConfig.DefaultValue, typeof(object))
                            }
                    );

                    Expression<Func<object, object>> converterExpr = (v) => propMapConfig.Converter(v);
                    getValueExpr = Expression.Invoke(converterExpr, getValueExpr);

                    var memberBinding = Expression.Bind(propMapConfig.Info, Expression.Convert(getValueExpr, propMapConfig.Info.PropertyType));
                    memberBindings.Add(memberBinding);
                }
            }

            var newItemExpr = Expression.New(typeof(TDto));
            var memberInitExpr = Expression.MemberInit(newItemExpr, memberBindings);

            var lambda = Expression.Lambda<Func<TDto>>(memberInitExpr);
            return lambda.Compile();
        }

        public static Func<TDto> CreateDefaultMapFunc(ReadHelper helper, string columnsPrefix = null)
        {
            return CreateMapFunc(helper, _defaultPropMapConfigs, columnsPrefix ?? DefaultColumnsPrefix);
        }
        #endregion

        protected Dictionary<string, PropMapConfig> _propMapConfigs;
        internal string ColumnsPrefix { get; private set; }
        internal string[] OrderedKeyPropNames { get; private set; }

        internal PropMapConfig GetInnerPropMapConfig(string propName)
        {
            if (_propMapConfigs.TryGetValue(propName, out PropMapConfig cfg))
            {
                return cfg;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        internal bool TryGetInnerPropMapConfig(string propName, out PropMapConfig result)
        {
            if (_propMapConfigs.TryGetValue(propName, out PropMapConfig cfg))
            {
                result = cfg;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private void Init(string columnsPrefix = null)
        {
            ColumnsPrefix = columnsPrefix;
            _propMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
            foreach (var defaultPropMapConfig in _defaultPropMapConfigs)
            {
                _propMapConfigs.Add(defaultPropMapConfig.Key, defaultPropMapConfig.Value.Clone());
            }
            if (DefaultOrderedKeyPropNames != null)
            {
                Array.Copy(DefaultOrderedKeyPropNames, OrderedKeyPropNames, DefaultOrderedKeyPropNames.Length);
            }
        }

        public DtoMapper(string columnsPrefix = null)
        {
            Init(columnsPrefix);
        }

        public DtoMapper<TDto> MapProp(string propertyName, Func<object, object> converter)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(string propertyName, string columnName, Func<object, object> converter = null)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnName = columnName;
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(string propertyName, int columnOrdinal, Func<object, object> converter = null)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnOrdinal = columnOrdinal;
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> SetKeyPropNames(params string[] keyPropNames)
        {
            OrderedKeyPropNames = keyPropNames;
            return this;
        }

        public Func<TDto> CreateMapFunc(ReadHelper helper, string columnsPrefix = null)
        {
            return CreateMapFunc(helper, _propMapConfigs, columnsPrefix ?? ColumnsPrefix ?? DefaultColumnsPrefix);
        }

        public int? ExtractOrdinal(ReadHelper helper, string propName, string columnsPrefix = null)
        {
            return ExtractOrdinal(_propMapConfigs, helper.ColumnNamesToOrdinals, propName, columnsPrefix);
        }

        public DtoMapper<TDto> Clone()
        {
            var clone = new DtoMapper<TDto>(this.ColumnsPrefix);
            clone.OrderedKeyPropNames = this.OrderedKeyPropNames.ToArray();
            foreach (var propMapConfig in this._propMapConfigs)
            {
                clone._propMapConfigs.Add(propMapConfig.Key, propMapConfig.Value.Clone());
            }
            return clone;
        }
    }
}
