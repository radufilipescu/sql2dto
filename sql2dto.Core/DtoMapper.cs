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
    public partial class DtoMapper<TDto> //where TDto : new()
    {
        internal static bool ImplementsIOnDtoRead { get; private set; }

        public static DtoMapper<TDto> Default { get; private set; }

        static DtoMapper()
        {
            var propMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
            var dtoType = typeof(TDto);

            ImplementsIOnDtoRead = typeof(IOnDtoRead).IsAssignableFrom(dtoType);

            string columnsPrefix = null;
            var columnsPrefixAttr = dtoType.GetTypeInfo().GetCustomAttribute<ColumnsPrefixAttribute>();
            if (columnsPrefixAttr != null)
            {
                columnsPrefix = columnsPrefixAttr.Value;
            }

            var ctorInfo = dtoType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(k => k.GetParameters().Length)
                .FirstOrDefault();

            if (ctorInfo == null)
            {
                bool containsPrivateCtors = dtoType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Length > 0;
                if (containsPrivateCtors)
                {
                    throw new Exception($"Type '{typeof(TDto).FullName}' must have at least one public constructor or no constructors at all");
                }
            }

            var keyPropMapConfigs = new List<PropMapConfig>();
            foreach (var propInfo in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propInfo.GetCustomAttribute<IgnorePropAttribute>() != null)
                {
                    continue;
                }

                if (!propInfo.PropertyType.GetTypeInfo().IsValueType)
                {
                    if (propInfo.PropertyType != typeof(string))
                    {
                        continue;
                    }
                }

                var propMapConfig = new PropMapConfig(propInfo);

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

            var useCtor = (ctorInfo?.GetParameters()?.Length ?? 0) > 0;

            Default = new DtoMapper<TDto>()
            {
                IsDefaultMapper = true,
                ColumnsPrefix = columnsPrefix,
                _ctorInfo = useCtor ? ctorInfo : null,
                _propMapConfigs = propMapConfigs,
                OrderedKeyPropNames = orderedKeyPropNames
            };
        }

        internal Func<TDto> CreateMapFunc(ReadHelper helper, string columnsPrefix = null)
        {
            //return CreateMapFunc(helper, _ctorMapParamConfigs, _propMapConfigs, IsFetchTolerant, columnsPrefix ?? ColumnsPrefix ?? Default.ColumnsPrefix);
            var finalColumnsPrefix = columnsPrefix ?? ColumnsPrefix ?? Default.ColumnsPrefix;

            var readHelperConstExpr = Expression.Constant(helper, typeof(ReadHelper));

            var ctorParametersExpressions = new List<Expression>();
            var usedPropsInCtor = new HashSet<string>();
            if (_ctorInfo != null)
            {
                foreach (var ctorParameter in _ctorInfo.GetParameters())
                {
                    if (!_propMapConfigs.TryGetValue(ctorParameter.Name, out PropMapConfig propMapConfig))
                    {
                        throw new Exception($"Parameter '{ctorParameter.Name}' from type '{typeof(TDto).Name}' constructor does not have a matching property."
                            + " Either add a matching property or remove the constructor parameter.");
                    }

                    if (propMapConfig.Ignored)
                    {
                        throw new Exception(
                            $"Constructor parameter '{typeof(TDto).Name}.{ctorParameter.Name}'" 
                            + $" is using a matching property '{typeof(TDto).Name}.{ctorParameter.Name}' that is ignored."
                            + " Either remove the constructor parameter, or remove the Ignore setting on the property");
                    }

                    usedPropsInCtor.Add(propMapConfig.Info.Name);

                    int? ordinal = ExtractOrdinal(helper.ColumnNamesToOrdinals, propMapConfig, finalColumnsPrefix);
                    if (!ordinal.HasValue)
                    {
                        throw new Exception($"The constructor parameter '{typeof(TDto).Name}.{ctorParameter.Name}'"
                            + $" matched to property '{typeof(TDto).Name}.{propMapConfig.Info.Name}'"
                            + $" could not be read because column '{finalColumnsPrefix ?? ""}{propMapConfig.ColumnName ?? propMapConfig.Info.Name}'"
                            + " was not found within the IDataReader's columns. Make sure the column is projected into the query.");
                    }

                    CreateGetColumnValueExpression(readHelperConstExpr, ordinal, propMapConfig,
                        (getValueExpression) =>
                        {
                            if (propMapConfig.Converter == null)
                            {
                                ctorParametersExpressions.Add(getValueExpression);
                            }
                            else
                            {
                                ctorParametersExpressions.Add(Expression.Convert(getValueExpression, propMapConfig.Info.PropertyType));
                            }
                        });
                }
            }

            List<MemberBinding> memberBindings = new List<MemberBinding>();
            foreach (var propMapConfig in _propMapConfigs.Values)
            {
                if (!propMapConfig.Info.CanWrite)
                {
                    continue;
                }

                if (propMapConfig.Ignored)
                {
                    continue;
                }

                if (usedPropsInCtor.Contains(propMapConfig.Info.Name))
                {
                    continue;
                }

                int? ordinal = ExtractOrdinal(helper.ColumnNamesToOrdinals, propMapConfig, finalColumnsPrefix);
                if (!ordinal.HasValue)
                {
                    if (IsFetchTolerant)
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception($"The property '{typeof(TDto).Name}.{propMapConfig.Info.Name}'"
                            + $" could not be read because column '{finalColumnsPrefix ?? ""}{propMapConfig.ColumnName ?? propMapConfig.Info.Name}'"
                            + " was not found within the IDataReader's columns. Make sure the column is projected into the query.");
                    }
                }

                CreateGetColumnValueExpression(readHelperConstExpr, ordinal, propMapConfig, 
                    (getValueExpression) =>
                    {
                        if (propMapConfig.Converter == null)
                        {
                            var memberBinding = Expression.Bind(propMapConfig.Info, getValueExpression);
                            memberBindings.Add(memberBinding);
                        }
                        else
                        {
                            var memberBinding = Expression.Bind(propMapConfig.Info, Expression.Convert(getValueExpression, propMapConfig.Info.PropertyType));
                            memberBindings.Add(memberBinding);
                        }
                    });
            }

            var newItemExpr = _ctorInfo == null
                ? Expression.New(typeof(TDto))
                : Expression.New(_ctorInfo, ctorParametersExpressions);

            var memberInitExpr = Expression.MemberInit(newItemExpr, memberBindings);

            var lambda = Expression.Lambda<Func<TDto>>(memberInitExpr);
            return lambda.Compile();
        }

        private void CreateGetColumnValueExpression(ConstantExpression readHelperConstExpr, int? ordinal, PropMapConfig propMapConfig, Action<Expression> callback)
        {
            if (propMapConfig.Converter == null)
            {
                Expression getValueExpr = Expression.Call(readHelperConstExpr,
                    propMapConfig.ReadHelperGetterMethodInfo, new Expression[]
                        {
                                Expression.Constant(ordinal, typeof(int))
                        }
                );

                callback(getValueExpr);
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

                callback(getValueExpr);
            }
        }

        public bool IsDefaultMapper { get; private set; }

        protected ConstructorInfo _ctorInfo;
        internal ConstructorInfo CtorInfo => _ctorInfo;

        protected Dictionary<string, PropMapConfig> _propMapConfigs;
        internal IReadOnlyDictionary<string, PropMapConfig> PropMapConfigs => _propMapConfigs;

        internal string ColumnsPrefix { get; private set; }

        internal string[] OrderedKeyPropNames { get; private set; }

        private DtoMapper(string columnsPrefix = null)
        {
            IsDefaultMapper = false;
            ColumnsPrefix = columnsPrefix;
            OrderedKeyPropNames = null;
            _propMapConfigs = new Dictionary<string, PropMapConfig>(StringComparer.OrdinalIgnoreCase);
        }

        public bool IsFetchTolerant { get; private set; }
        public DtoMapper<TDto> SetFetchTolerance(bool tolerance)
        {
            IsFetchTolerant = tolerance;
            return this;
        }

        public DtoMapper<TDto> UseConstructor(ConstructorInfo ctorInfo)
        {
            if (ctorInfo == null)
            {
                throw new ArgumentNullException(nameof(ctorInfo));
            }

            if (ctorInfo.DeclaringType != typeof(TDto))
            {
                throw new ArgumentException($"Type '{typeof(TDto).FullName}' does not contain this constructor", nameof(ctorInfo));
            }

            if (!ctorInfo.IsPublic)
            {
                throw new Exception($"DtoMapper<{typeof(TDto).FullName}> must use a public constructor");
            }

            _ctorInfo = ctorInfo;
            return this;
        }

        public DtoMapper<TDto> IgnoreProps(params Expression<Func<TDto, object>>[] propertySelectors)
        {
            return this.IgnoreProps(propertySelectors.Select(s => InternalUtils.GetPropertyName(s)).ToArray());
        }

        public DtoMapper<TDto> IgnoreProps(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (_propMapConfigs.TryGetValue(propertyName, out PropMapConfig config))
                {
                    config.Ignored = true;
                }
                else
                {
                    throw new ArgumentException("Property not found", nameof(propertyNames));
                }
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
            clone.IsFetchTolerant = this.IsFetchTolerant;
            foreach (var propMapConfig in this._propMapConfigs)
            {
                clone._propMapConfigs.Add(propMapConfig.Key, propMapConfig.Value.Clone());
            }
            return clone;
        }
    }
}
