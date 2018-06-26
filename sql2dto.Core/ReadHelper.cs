using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Text;

namespace sql2dto.Core
{
    public class ReadHelper : IDisposable
    {
        #region STATIC
        internal static MethodInfo GetValueOrDefaultMethodInfo { get; private set; }
        private static Dictionary<string, MethodInfo> _cachedGetterOrdinalMethodInfos;

        internal static MethodInfo GetGetterOrdinalMethodInfo(string getterMethodName)
        {
            if (_cachedGetterOrdinalMethodInfos.TryGetValue(getterMethodName, out MethodInfo mi))
            {
                return mi;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(getterMethodName), $"MethodInfo '{getterMethodName}' was not found on ReadHelper's cache");
            }
        }

        static ReadHelper()
        {
            GetValueOrDefaultMethodInfo = typeof(ReadHelper).GetMethod("GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic);
            _cachedGetterOrdinalMethodInfos = new Dictionary<string, MethodInfo>();
            foreach (MethodInfo getterMethodInfo in typeof(ReadHelper).GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (getterMethodInfo.GetCustomAttribute<CacheGetterOrdinalMethodInfoAttribute>() == null)
                {
                    continue;
                }

                if (_cachedGetterOrdinalMethodInfos.ContainsKey(getterMethodInfo.Name))
                {
                    continue;
                }

                _cachedGetterOrdinalMethodInfos.Add(getterMethodInfo.Name, getterMethodInfo);
            }
        }
        #endregion

        public IDataReader Reader { get; private set; }

        public ReadOnlyDictionary<string, int> ColumnNamesToOrdinals { get; private set; }
        public ReadOnlyDictionary<int, Type> ColumnOrdinalsToTypes { get; private set; }
        public ReadOnlyDictionary<int, TypeCode> ColumnOrdinalsToTypeCodes { get; private set; }

        internal event EventHandler ColumnMappingsChanged;

        private IReadHelperSettings _settings;

        public ReadHelper(IDataReader reader, IReadHelperSettings settings = null)
        {
            Reader = reader;
            _settings = settings;
            SetupColumnMappings();
        }

        private void SetupColumnMappings()
        {
            var columnNamesToOrdinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var columnOrdinalsToTypes = new Dictionary<int, Type>();
            var columnOrdinalsToTypeCodes = new Dictionary<int, TypeCode>();

            for (int i = 0; i < Reader.FieldCount; i++)
            {
                string colName = Reader.GetName(i);
                if (columnNamesToOrdinals.TryGetValue(colName, out int ordinal))
                {
                    int ocurrence = 1;
                    string newColName;
                    while (columnNamesToOrdinals.ContainsKey(newColName = $"{colName}_{ocurrence}"))
                    {
                        ocurrence++;
                    }

                    columnNamesToOrdinals.Add(newColName, i);
                }
                else
                {
                    columnNamesToOrdinals.Add(colName, i);
                }

                Type fieldType = Reader.GetFieldType(i);
                columnOrdinalsToTypes.Add(i, fieldType);

                TypeCode fieldTypeCode = Type.GetTypeCode(fieldType);
                columnOrdinalsToTypeCodes.Add(i, fieldTypeCode);
            }

            this.ColumnNamesToOrdinals = new ReadOnlyDictionary<string, int>(columnNamesToOrdinals);
            this.ColumnOrdinalsToTypes = new ReadOnlyDictionary<int, Type>(columnOrdinalsToTypes);
            this.ColumnOrdinalsToTypeCodes = new ReadOnlyDictionary<int, TypeCode>(columnOrdinalsToTypeCodes);

            ColumnMappingsChanged?.Invoke(this, new EventArgs());
        }

        public bool Read()
        {
            return Reader.Read();
        }

        public bool NextResult()
        {
            bool result = Reader.NextResult();
            if (result)
            {
                SetupColumnMappings();
            }
            return result;
        }

        public void Dispose()
        {
            Reader.Dispose();
        }

        internal object GetValueOrDefault(int ordinal, object defaultValue)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return defaultValue;
            }
            return Reader.GetValue(ordinal);
        }

        #region DATA GETTERS
        #region GetValue
        public object GetValue(int ordinal)
        {
            return Reader.GetValue(ordinal);
        }

        public T GetValue<T>(int ordinal)
        {
            return (T)Reader.GetValue(ordinal);
        }

        public object GetNullableValue(int ordinal)
        {
            return Reader.IsDBNull(ordinal) ? null : Reader.GetValue(ordinal);
        }

        public object GetValue<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetValue(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public object GetNullableValue<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.IsDBNull(ordinal.Value) ? null : Reader.GetValue(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public object GetValue<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetValue(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public object GetNullableValue<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.IsDBNull(ordinal.Value) ? null : Reader.GetValue(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetBoolean
        [CacheGetterOrdinalMethodInfo]
        public bool GetBoolean(int ordinal)
        {
            TypeCode columnTypeCode = ColumnOrdinalsToTypeCodes[ordinal];
            if (columnTypeCode == TypeCode.Boolean)
            {
                return Reader.GetBoolean(ordinal);
            }

            if (_settings != null
                && _settings.BooleanTranslator != null)
            {
                switch (columnTypeCode)
                {
                    case TypeCode.Int16:
                        {
                            if (_settings.BooleanTranslator.FromInt16ToBool != null)
                            {
                                var value = Reader.GetInt16(ordinal);
                                return _settings.BooleanTranslator.FromInt16ToBool(value);
                            }
                        }
                        break;
                    case TypeCode.Int32:
                        {
                            if (_settings.BooleanTranslator.FromInt32ToBool != null)
                            {
                                var value = Reader.GetInt32(ordinal);
                                return _settings.BooleanTranslator.FromInt32ToBool(value);
                            }
                        }
                        break;
                    case TypeCode.Int64:
                        {
                            if (_settings.BooleanTranslator.FromInt64ToBool != null)
                            {
                                var value = Reader.GetInt64(ordinal);
                                return _settings.BooleanTranslator.FromInt64ToBool(value);
                            }
                        }
                        break;

                    case TypeCode.Double:
                        {
                            if (_settings.BooleanTranslator.FromDoubleToBool != null)
                            {
                                var value = Reader.GetDouble(ordinal);
                                return _settings.BooleanTranslator.FromDoubleToBool(value);
                            }
                        }
                        break;
                    case TypeCode.Single:
                        {
                            if (_settings.BooleanTranslator.FromFloatToBool != null)
                            {
                                var value = Reader.GetFloat(ordinal);
                                return _settings.BooleanTranslator.FromFloatToBool(value);
                            }
                        }
                        break;
                    case TypeCode.Decimal:
                        {
                            if (_settings.BooleanTranslator.FromDecimalToBool != null)
                            {
                                var value = Reader.GetDecimal(ordinal);
                                return _settings.BooleanTranslator.FromDecimalToBool(value);
                            }
                        }
                        break;

                    case TypeCode.Char:
                        {
                            if (_settings.BooleanTranslator.FromCharToBool != null)
                            {
                                var value = Reader.GetChar(ordinal);
                                return _settings.BooleanTranslator.FromCharToBool(value);
                            }
                        }
                        break;
                    case TypeCode.String:
                        {
                            if (_settings.BooleanTranslator.FromStringToBool != null)
                            {
                                var value = Reader.GetString(ordinal);
                                return _settings.BooleanTranslator.FromStringToBool(value);
                            }
                        }
                        break;
                }
            }

            throw new InvalidCastException($"ReadHelper could not cast '{columnTypeCode}' to '{typeof(Boolean).FullName}'");
        }

        [CacheGetterOrdinalMethodInfo]
        public bool? GetNullableBoolean(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return this.GetBoolean(ordinal);
            }
        }

        public bool GetBoolean(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return this.GetBoolean(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public bool? GetNullableBoolean(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return this.GetBoolean(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public bool GetBoolean<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return this.GetBoolean(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public bool? GetNullableBoolean<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return this.GetBoolean(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public bool GetBoolean<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return this.GetBoolean(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public bool? GetNullableBoolean<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return this.GetBoolean(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetByte
        public byte GetByte(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetByte(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public byte GetByte<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetByte(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public byte GetByte<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetByte(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetBytes
        public long GetBytes(string columnName, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetBytes(ordinal, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public long GetBytes<TDto>(string propName, long fieldOffset, byte[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, null);
            if (ordinal.HasValue)
            {
                return Reader.GetBytes(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetBytes<TDto>(string propName, string columnsPrefix, long fieldOffset, byte[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetBytes(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetBytes<TDto>(DtoMapper<TDto> mapper, string propName, long fieldOffset, byte[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, null);
            if (ordinal.HasValue)
            {
                return Reader.GetBytes(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetBytes<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix, long fieldOffset, byte[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetBytes(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetChar
        public char GetChar(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetChar(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public char GetChar<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetChar(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public char GetChar<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetChar(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetChars
        public long GetChars(string columnName, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetChars(ordinal, fieldoffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public long GetChars<TDto>(string propName, long fieldOffset, char[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, null);
            if (ordinal.HasValue)
            {
                return Reader.GetChars(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetChars<TDto>(string propName, string columnsPrefix, long fieldOffset, char[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetChars(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetChars<TDto>(DtoMapper<TDto> mapper, string propName, long fieldOffset, char[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, null);
            if (ordinal.HasValue)
            {
                return Reader.GetChars(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetChars<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix, long fieldOffset, char[] buffer, int bufferoffset, int length) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetChars(ordinal.Value, fieldOffset, buffer, bufferoffset, length);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetDataTypeName
        public string GetDataTypeName(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetDataTypeName(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public string GetDataTypeName<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDataTypeName(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public string GetDataTypeName<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDataTypeName(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetDateTime
        [CacheGetterOrdinalMethodInfo]
        public DateTime GetDateTime(int ordinal)
        {
            return Reader.GetDateTime(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public DateTime? GetNullableDateTime(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetDateTime(ordinal);
            }
        }

        public DateTime GetDateTime(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetDateTime(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public DateTime? GetNullableDateTime(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDateTime(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public DateTime GetDateTime<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDateTime(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public DateTime? GetNullableDateTime<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDateTime(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public DateTime GetDateTime<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDateTime(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public DateTime? GetNullableDateTime<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDateTime(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetDecimal
        [CacheGetterOrdinalMethodInfo]
        public decimal GetDecimal(int ordinal)
        {
            return Reader.GetDecimal(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public decimal? GetNullableDecimal(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetDecimal(ordinal);
            }
        }

        public decimal GetDecimal(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetDecimal(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public decimal? GetNullableDecimal(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDecimal(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public decimal GetDecimal<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDecimal(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public decimal? GetNullableDecimal<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDecimal(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public decimal GetDecimal<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDecimal(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public decimal? GetNullableDecimal<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDecimal(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetDouble
        [CacheGetterOrdinalMethodInfo]
        public double GetDouble(int ordinal)
        {
            return Reader.GetDouble(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public double? GetNullableDouble(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetDouble(ordinal);
            }
        }

        public double GetDouble(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetDouble(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public double? GetNullableDouble(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDouble(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public double GetDouble<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDouble(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public double? GetNullableDouble<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDouble(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public double GetDouble<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetDouble(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public double? GetNullableDouble<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetDouble(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetFieldType
        public Type GetFieldType(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetFieldType(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public Type GetFieldType<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetFieldType(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public Type GetFieldType<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetFieldType(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetFloat
        [CacheGetterOrdinalMethodInfo]
        public float GetFloat(int ordinal)
        {
            return Reader.GetFloat(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public float? GetNullableFloat(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetFloat(ordinal);
            }
        }

        public float GetFloat(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetFloat(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public float? GetNullableFloat(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetFloat(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public float GetFloat<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetFloat(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public float? GetNullableFloat<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetFloat(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public float GetFloat<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetFloat(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public float? GetNullableFloat<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetFloat(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetGuid
        [CacheGetterOrdinalMethodInfo]
        public Guid GetGuid(int ordinal)
        {
            return Reader.GetGuid(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public Guid? GetNullableGuid(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetGuid(ordinal);
            }
        }

        public Guid GetGuid(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetGuid(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public Guid? GetNullableGuid(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetGuid(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public Guid GetGuid<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetGuid(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public Guid? GetNullableGuid<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetGuid(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public Guid GetGuid<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetGuid(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public Guid? GetNullableGuid<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetGuid(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetInt16
        [CacheGetterOrdinalMethodInfo]
        public short GetInt16(int ordinal)
        {
            return Reader.GetInt16(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public short? GetNullableInt16(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetInt16(ordinal);
            }
        }

        public short GetInt16(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetInt16(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public short? GetNullableInt16(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt16(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public short GetInt16<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt16(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public short? GetNullableInt16<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt16(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public short GetInt16<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt16(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public short? GetNullableInt16<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt16(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetInt32
        [CacheGetterOrdinalMethodInfo]
        public int GetInt32(int ordinal)
        {
            return Reader.GetInt32(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public int? GetNullableInt32(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetInt32(ordinal);
            }
        }

        public int GetInt32(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetInt32(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public int? GetNullableInt32(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt32(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public int GetInt32<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt32(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public int? GetNullableInt32<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt32(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public int GetInt32<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt32(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public int? GetNullableInt32<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt32(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetInt64
        [CacheGetterOrdinalMethodInfo]
        public long GetInt64(int ordinal)
        {
            return Reader.GetInt64(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public long? GetNullableInt64(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetInt64(ordinal);
            }
        }

        public long GetInt64(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetInt64(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public long? GetNullableInt64(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt64(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public long GetInt64<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt64(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long? GetNullableInt64<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt64(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long GetInt64<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetInt64(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public long? GetNullableInt64<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetInt64(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetString
        [CacheGetterOrdinalMethodInfo]
        public string GetString(int ordinal)
        {
            return Reader.GetString(ordinal);
        }

        [CacheGetterOrdinalMethodInfo]
        public string GetNullableString(int ordinal)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return Reader.GetString(ordinal);
            }
        }

        public string GetString(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetString(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public string GetNullableString(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                if (Reader.IsDBNull(ordinal))
                {
                    return null;
                }
                else
                {
                    return Reader.GetString(ordinal);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public string GetString<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetString(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public string GetNullableString<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetString(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public string GetString<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.GetString(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public string GetNullableString<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                if (Reader.IsDBNull(ordinal.Value))
                {
                    return null;
                }
                else
                {
                    return Reader.GetString(ordinal.Value);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        #region GetValues
        public int GetValues(object[] values)
        {
            return Reader.GetValues(values);
        }
        #endregion

        #region IsDBNull
        public bool IsDBNull(int ordinal)
        {
            return Reader.IsDBNull(ordinal);
        }

        public bool IsDBNull(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.IsDBNull(ordinal);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
        }

        public bool IsDBNull<TDto>(string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = DtoMapper<TDto>.ExtractDefaultOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.IsDBNull(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public bool IsDBNull<TDto>(DtoMapper<TDto> mapper, string propName, string columnsPrefix = null) where TDto : new()
        {
            int? ordinal = mapper.ExtractOrdinal(this, propName, columnsPrefix);
            if (ordinal.HasValue)
            {
                return Reader.IsDBNull(ordinal.Value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
        #endregion

        private class CacheGetterOrdinalMethodInfoAttribute : Attribute { }
        #endregion

        #region Fetch Operations
        public FetchOp<TDto> Fetch<TDto>()
            where TDto : new()
        {
            return FetchOp<TDto>.Create(this);
        }

        public FetchOp<TDto> Fetch<TDto>(DtoMapper<TDto> mapper)
            where TDto : new()
        {
            return FetchOp<TDto>.Create(this, mapper);
        }

        public FetchOp<TDto> Fetch<TDto>(string columnsPrefix)
            where TDto : new()
        {
            return FetchOp<TDto>.Create(columnsPrefix, this);
        }

        public FetchOp<TDto> Fetch<TDto>(string columnsPrefix, DtoMapper<TDto> mapper)
            where TDto : new()
        {
            return FetchOp<TDto>.Create(columnsPrefix, this, mapper);
        }
        #endregion

        public Dictionary<string, object> FetchCurrentRowDataAsDictionary()
        {
            if (Reader.IsClosed)
            {
                throw new InvalidOperationException("Reader is closed");
            }

            var result = new Dictionary<string, object>();
            foreach (var kvp in ColumnNamesToOrdinals)
            {
                result[kvp.Key] = GetValue(kvp.Value);
            }
            return result;
        }

        public Dictionary<string, List<object>> FetchAllDataAsDictionary()
        {
            var result = new Dictionary<string, List<object>>();
            foreach (var header in ColumnNamesToOrdinals.Keys)
            {
                result.Add(header, new List<object>());
            }
            while (Reader.Read())
            {
                foreach (var kvp in ColumnNamesToOrdinals)
                {
                    result[kvp.Key].Add(GetValue(kvp.Value));
                }
            }
            return result;
        }

        public List<object> FetchColumnDataAsList(string columnName)
        {
            if (!ColumnNamesToOrdinals.ContainsKey(columnName))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
            int columnOrdinal = ColumnNamesToOrdinals[columnName];
            var result = new List<object>();
            while (Reader.Read())
            {
                result.Add(GetValue(columnOrdinal));
            }
            return result;
        }

        public List<object> FetchColumnDataAsList(int columnOrdinal)
        {
            var result = new List<object>();
            while (Reader.Read())
            {
                result.Add(GetValue(columnOrdinal));
            }
            return result;
        }

        public List<T> FetchColumnDataAsList<T>(string columnName)
        {
            if (!ColumnNamesToOrdinals.ContainsKey(columnName))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName));
            }
            int columnOrdinal = ColumnNamesToOrdinals[columnName];
            var result = new List<T>();
            while (Reader.Read())
            {
                result.Add(GetValue<T>(columnOrdinal));
            }
            return result;
        }

        public List<T> FetchColumnDataAsList<T>(int columnOrdinal)
        {
            var result = new List<T>();
            while (Reader.Read())
            {
                result.Add(GetValue<T>(columnOrdinal));
            }
            return result;
        }
    }
}
