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
        internal static MethodInfo GetValueOrdinalDefaultMethodInfo { get; private set; }

        static ReadHelper()
        {
            GetValueOrdinalDefaultMethodInfo = typeof(ReadHelper).GetMethod("GetValue", new Type[] { typeof(int), typeof(object) });
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

        #region GetValue
        public object GetValue(int ordinal)
        {
            return Reader.GetValue(ordinal);
        }

        public object GetValue(int ordinal, object defaultValue)
        {
            if (Reader.IsDBNull(ordinal))
            {
                return defaultValue;
            }
            return Reader.GetValue(ordinal);
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
        #endregion

        #region GetBoolean
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
        #endregion

        #region GetDecimal
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
        #endregion

        #region GetDouble
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
        #endregion

        #region GetGuid
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
        #endregion

        #region GetInt16
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
        #endregion

        #region GetInt32
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
        #endregion

        #region GetInt64
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
        #endregion

        #region GetString
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
        #endregion

        #region GetValues
        public int GetValues(object[] values)
        {
            return Reader.GetValues(values);
        }
        #endregion

        #region IsDBNull
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
    }
}
