using System;
using System.Collections.Generic;
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

        public Dictionary<string, int> ColumnNamesToOrdinals { get; private set; }

        internal event EventHandler ColumnNamesToOrdinalsChanged;

        public ReadHelper(IDataReader reader)
        {
            Reader = reader;
            SetupColumnNamesToOrdinals();
        }

        private void SetupColumnNamesToOrdinals()
        {
            ColumnNamesToOrdinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                string colName = Reader.GetName(i);
                if (ColumnNamesToOrdinals.TryGetValue(colName, out int ordinal))
                {
                    int ocurrence = 1;
                    string newColName;
                    while (ColumnNamesToOrdinals.ContainsKey(newColName = $"{colName}_{ocurrence}"))
                    {
                        ocurrence++;
                    }

                    ColumnNamesToOrdinals.Add(newColName, i);
                }
                else
                {
                    ColumnNamesToOrdinals.Add(colName, i);
                }
            }
            ColumnNamesToOrdinalsChanged?.Invoke(this, new EventArgs());
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
                SetupColumnNamesToOrdinals();
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
            return Reader.GetBoolean(ordinal);
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
                return Reader.GetBoolean(ordinal);
            }
        }

        public bool GetBoolean(string columnName)
        {
            if (ColumnNamesToOrdinals.TryGetValue(columnName, out int ordinal))
            {
                return Reader.GetBoolean(ordinal);
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
                    return Reader.GetBoolean(ordinal);
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
                return Reader.GetBoolean(ordinal.Value);
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
                    return Reader.GetBoolean(ordinal.Value);
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
                return Reader.GetBoolean(ordinal.Value);
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
                    return Reader.GetBoolean(ordinal.Value);
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

        #region No Keys
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
        #endregion

        #region 1 Key
        public FetchOp<TDto> Fetch<TDto, TKey>(ReadHelper readHelper)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(this, mapper);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(string columnsPrefix, ReadHelper readHelper)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(columnsPrefix, this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(columnsPrefix, this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(ReadHelper readHelper, string keyPropName)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(this, keyPropName);
        }

        public FetchOp<TDto> Fetch<TDto, TKey>(string columnsPrefix, ReadHelper readHelper, string keyPropName)
            where TDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return FetchOp<TDto>.Create<TKey>(columnsPrefix, this, keyPropName);
        }
        #endregion

        #region 2 Keys
        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(ReadHelper readHelper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(this, mapper);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(columnsPrefix, this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(columnsPrefix, this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(ReadHelper readHelper, string keyPropName1, string keyPropName2)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(this, keyPropName1, keyPropName2);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2>(columnsPrefix, this, keyPropName1, keyPropName2);
        }
        #endregion

        #region 3 Keys
        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(ReadHelper readHelper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(this, mapper);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this, keyReadFunc);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(this, keyPropName1, keyPropName2, keyPropName3);
        }

        public FetchOp<TDto> Fetch<TDto, TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3)
            where TDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return FetchOp<TDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this, keyPropName1, keyPropName2, keyPropName3);
        }
        #endregion

        #endregion
    }
}
