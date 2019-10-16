using sql2dto.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace sql2dto.Core
{
    public partial class DtoMapper<TDto>
    {
        internal static bool ImplementsIOnDtoRead { get; private set; }

        public static DtoMapper<TDto> Default { get; private set; }

        static DtoMapper()
        {
            var fieldMapConfigs = new Dictionary<string, FieldMapConfig>(StringComparer.OrdinalIgnoreCase);
            var propMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
            var dtoType = typeof(TDto);

            ImplementsIOnDtoRead = typeof(IOnDtoRead).IsAssignableFrom(dtoType);

            string columnsPrefix = null;
            var columnsPrefixAttr = dtoType.GetTypeInfo().GetCustomAttribute<ColumnsPrefixAttribute>();
            if (columnsPrefixAttr != null)
            {
                columnsPrefix = columnsPrefixAttr.Value;
            }

            var ctorInfo = dtoType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(ci => ci.GetParameters().Length)
                .FirstOrDefault();

            foreach (var fieldInfo in dtoType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                object defaultValue = null;

                if (!fieldInfo.FieldType.GetTypeInfo().IsValueType)
                {
                    if (fieldInfo.FieldType != typeof(string))
                    {
                        continue;
                    }
                }
                else
                {
                    defaultValue = Activator.CreateInstance(fieldInfo.FieldType);
                }

                var fieldMapConfig = new FieldMapConfig(fieldInfo, defaultValue);

                fieldMapConfigs.Add(fieldMapConfig.Info.Name, fieldMapConfig);
            }

            var keyPropMapConfigs = new List<PropMapConfig>();
            foreach (var propInfo in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
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
                    propMapConfig.DeclarationOrder = propMapAttribute.DeclarationOrder;
                    propMapConfig.ColumnName = propMapAttribute.ColumnName;
                    propMapConfig.ColumnOrdinal = propMapAttribute.ColumnOrdinal;
                    propMapConfig.IsKey = propMapAttribute.IsKey;
                    propMapConfig.IsNullableKey = propMapAttribute.IsNullableKey;
                }

                var converterAttribute = propInfo.GetCustomAttribute<ConverterAttribute>();
                if (converterAttribute != null)
                {
                    propMapConfig.Converter = converterAttribute.Converter;
                }

                if (propMapConfig.IsKey || propMapConfig.IsNullableKey)
                {
                    keyPropMapConfigs.Add(propMapConfig);
                }

                propMapConfigs.Add(propMapConfig.Info.Name, propMapConfig);
            }

            string[] orderedKeyPropNames = null;
            var keyPropsAttribute = dtoType.GetCustomAttribute<KeyPropsAttribute>();
            if (keyPropsAttribute != null)
            {
                orderedKeyPropNames = keyPropsAttribute.KeyPropNames;
            }
            else if (keyPropMapConfigs.Count > 0)
            {
                orderedKeyPropNames = keyPropMapConfigs
                    .OrderBy(c => c.DeclarationOrder)
                    .Select(c => c.Info.Name)
                    .ToArray();
            }

            Default = new DtoMapper<TDto>()
            {
                IsDefaultMapper = true,
                ColumnsPrefix = columnsPrefix,
                _ctorInfo = ctorInfo,
                _propMapConfigs = propMapConfigs,
                _fieldMapConfigs = fieldMapConfigs,
                OrderedKeyPropNames = orderedKeyPropNames
            };
        }

        private static Func<TDto> CreateMapFunc(ReadHelper helper, Dictionary<string, PropMapConfig> propertyMapConfigs, ConstructorInfo ctorInfo, 
            string columnsPrefix = null, 
            Dictionary<string, object> injectedValues = null)
        {
            var readHelperConstExpr = Expression.Constant(helper, typeof(ReadHelper));

            List<PropMapConfig> backedPropMapConfigs = new List<PropMapConfig>();

            List<MemberBinding> memberBindings = new List<MemberBinding>();
            foreach (var propMapConfig in propertyMapConfigs.Values)
            {
                if (propMapConfig.BackingFieldMapConfig != null)
                {
                    backedPropMapConfigs.Add(propMapConfig);
                    continue;
                }
                else if (!propMapConfig.Info.CanWrite)
                {
                    throw new Exception(
                        $"Property '{propMapConfig.Info.Name}' cannot be written to. Either create a setter for it or use a backing field. "
                        + "If property is inherited, use a protected setter or a protected backing field.");
                }

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

            var ctorParamExprs = injectedValues?.Values.Select(v => Expression.Constant(v)).ToArray() ?? new ConstantExpression[0];

            var newItemExpr = (ctorInfo == null || ctorInfo.GetParameters().Length == 0)
                ? Expression.New(typeof(TDto))
                : Expression.New(ctorInfo, ctorParamExprs);
            var memberInitExpr = Expression.MemberInit(newItemExpr, memberBindings);

            var lambda = Expression.Lambda<Func<TDto>>(memberInitExpr);
            var compiledLambda = lambda.Compile();

            if (backedPropMapConfigs.Count > 0)
            {
                return new Func<TDto>(() =>
                {
                    var dto = compiledLambda();
                    foreach (var backedPropMapConfig in backedPropMapConfigs)
                    {
                        var ordinal = ExtractOrdinal(helper.ColumnNamesToOrdinals, backedPropMapConfig, columnsPrefix);
                        var val = helper.GetNullableValue(ordinal.Value);
                        backedPropMapConfig.BackingFieldMapConfig.Info.SetValue(dto, val);
                    }
                    return dto;
                });
            }

            return compiledLambda;
        }

        public bool IsDefaultMapper { get; private set; }
        protected Dictionary<string, PropMapConfig> _propMapConfigs;
        private Dictionary<string, FieldMapConfig> _fieldMapConfigs;
        private ConstructorInfo _ctorInfo;

        internal IReadOnlyDictionary<string, PropMapConfig> PropMapConfigs => _propMapConfigs;
        internal IReadOnlyDictionary<string, FieldMapConfig> FieldMapConfigs => _fieldMapConfigs;
        internal string ColumnsPrefix { get; private set; }
        internal string[] OrderedKeyPropNames { get; private set; }

        private DtoMapper(string columnsPrefix = null)
        {
            IsDefaultMapper = false;
            ColumnsPrefix = columnsPrefix;
            OrderedKeyPropNames = null;
            _propMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
        }

        public DtoMapper<TDto> BackingField(Expression<Func<TDto, object>> propertySelector, string fieldName)
        {
            return this.BackingField(InternalUtils.GetPropertyName(propertySelector), fieldName);
        }

        public DtoMapper<TDto> BackingField(string propertyName, string fieldName)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                if (_fieldMapConfigs.TryGetValue(fieldName, out FieldMapConfig fieldConfig))
                {
                    config.BackingFieldName = fieldName;
                    config.BackingFieldMapConfig = fieldConfig;
                }
                else
                {
                    throw new ArgumentException("Backing field not found", nameof(fieldName));
                }
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(Expression<Func<TDto, object>> propertySelector, string columnName)
        {
            return this.MapProp(InternalUtils.GetPropertyName(propertySelector), columnName);
        }

        public DtoMapper<TDto> MapProp(string propertyName, string columnName)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnName = columnName;
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(Expression<Func<TDto, object>> propertySelector, string columnName, Func<object, object> converter)
        {
            return this.MapProp(InternalUtils.GetPropertyName(propertySelector), columnName, converter);
        }

        public DtoMapper<TDto> MapProp(string propertyName, string columnName, Func<object, object> converter)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnName = columnName;
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(Expression<Func<TDto, object>> propertySelector, int columnOrdinal)
        {
            return this.MapProp(InternalUtils.GetPropertyName(propertySelector), columnOrdinal);
        }

        public DtoMapper<TDto> MapProp(string propertyName, int columnOrdinal)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnOrdinal = columnOrdinal;
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(Expression<Func<TDto, object>> propertySelector, int columnOrdinal, Func<object, object> converter)
        {
            return this.MapProp(InternalUtils.GetPropertyName(propertySelector), columnOrdinal, converter);
        }

        public DtoMapper<TDto> MapProp(string propertyName, int columnOrdinal, Func<object, object> converter)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.ColumnOrdinal = columnOrdinal;
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> MapProp(Expression<Func<TDto, object>> propertySelector, Func<object, object> converter)
        {
            return this.MapProp(InternalUtils.GetPropertyName(propertySelector), converter);
        }

        public DtoMapper<TDto> MapProp(string propertyName, Func<object, object> converter)
        {
            if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
            {
                config.Converter = converter;
            }
            else
            {
                throw new ArgumentException("Property not found", nameof(propertyName));
            }

            return this;
        }

        public DtoMapper<TDto> SetKeyProps(params Expression<Func<TDto, object>>[] keyPropSelectors)
        {
            return this.SetKeyProps(keyPropSelectors.Select(s => InternalUtils.GetPropertyName(s)).ToArray());
        }

        public DtoMapper<TDto> SetKeyProps(params string[] keyPropNames)
        {
            foreach (var keyPropName in keyPropNames)
            {
                if (!_propMapConfigs.ContainsKey(keyPropName))
                {
                    throw new ArgumentException("Property not found", nameof(keyPropName));
                }
            }
            OrderedKeyPropNames = keyPropNames;
            return this;
        }

        public DtoMapper<TDto> SetNullableKeyProps(params Expression<Func<TDto, object>>[] nullableKeyPropSelectors)
        {
            return this.SetNullableKeyProps(nullableKeyPropSelectors.Select(sel => (InternalUtils.GetPropertyName(sel), true)).ToArray());
        }

        public DtoMapper<TDto> SetNullableKeyProps(params (Expression<Func<TDto, object>>, bool)[] nullableKeyPropSelectors)
        {
            return this.SetNullableKeyProps(nullableKeyPropSelectors.Select(tuple => (InternalUtils.GetPropertyName(tuple.Item1), tuple.Item2)).ToArray());
        }

        public DtoMapper<TDto> SetNullableKeyProps(params string[] nullableKeyPropNames)
        {
            return this.SetNullableKeyProps(nullableKeyPropNames.Select(x => (x, true)).ToArray());
        }

        public DtoMapper<TDto> SetNullableKeyProps(params (string, bool)[] nullableKeyPropNames)
        {
            OrderedKeyPropNames = nullableKeyPropNames.Select(s => s.Item1).ToArray();
            foreach (var tuple in nullableKeyPropNames)
            {
                if (_propMapConfigs.TryGetValue(tuple.Item1, out PropMapConfig config))
                {
                    config.IsNullableKey = tuple.Item2;
                }
                else
                {
                    throw new ArgumentException("Property not found", nameof(tuple.Item1));
                }
            }
            return this;
        }

        public DtoMapper<TDto> SetColumnsPrefix(string columnsPrefix)
        {
            ColumnsPrefix = columnsPrefix;
            return this;
        }

        internal Func<TDto> CreateMapFunc(ReadHelper helper, string columnsPrefix = null, Dictionary<string, object> injectedValues = null)
        {
            return CreateMapFunc(helper, _propMapConfigs, _ctorInfo, columnsPrefix ?? ColumnsPrefix ?? Default.ColumnsPrefix, injectedValues);
        }

        internal int? ExtractOrdinal(ReadHelper helper, string propName, string columnsPrefix = null)
        {
            if (_propMapConfigs.TryGetValue(propName, out PropMapConfig config))
            {
                return ExtractOrdinal(helper.ColumnNamesToOrdinals, config, columnsPrefix);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName), $"Property map config for '{propName}' could not be found on type {typeof(TDto).FullName}");
            }
        }

        private static int? ExtractOrdinal(ReadOnlyDictionary<string, int> columnNamesToColumnOrdinals, PropMapConfig config, string columnsPrefix = null)
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

        public DtoMapper<TDto> Clone()
        {
            var clone = new DtoMapper<TDto>(this.ColumnsPrefix);
            clone.IsDefaultMapper = false;
            clone.OrderedKeyPropNames = this.OrderedKeyPropNames?.ToArray();
            foreach (var propMapConfig in this._propMapConfigs)
            {
                clone._propMapConfigs.Add(propMapConfig.Key, propMapConfig.Value.Clone());
            }
            return clone;
        }
    }
}
