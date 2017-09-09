using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace sql2dto.Core.UnitTests.Utils
{
    public class FakeDataReader : IDataReader
    {
        private int _currentReadResult;
        private int _currentAddResult;

        private List<string[]> _columnNames;
        private List<List<object[]>> _values;
        private int _currentRow;

        public FakeDataReader(params string[] columnNames)
        {
            _columnNames = new List<string[]> { columnNames };
            _values = new List<List<object[]>> { new List<object[]>() };
            _currentRow = -1;
        }

        public void AddNewResult(params string[] columnNames)
        {
            _currentAddResult++;
            _columnNames.Add(columnNames);
            _values.Add(new List<object[]>());
        }

        public void AddRow(params object[] values)
        {
            if (values.Length != _columnNames[_currentReadResult].Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            _values[_currentAddResult].Add(values);
        }

        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth => 0;

        public bool IsClosed => false;

        public int RecordsAffected => 0;

        public int FieldCount => _columnNames[_currentReadResult].Length;

        public void Close()
        {
            
        }

        public void Dispose()
        {
            
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(_values[_currentReadResult][_currentRow][i]);
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(_values[_currentReadResult][_currentRow][i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(_values[_currentReadResult][_currentRow][i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return _values[_currentReadResult][_currentRow][i].GetType().FullName;
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(_values[_currentReadResult][_currentRow][i]);
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(_values[_currentReadResult][_currentRow][i]);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(_values[_currentReadResult][_currentRow][i]);
        }

        public Type GetFieldType(int i)
        {
            return _values[_currentReadResult][_currentRow][i].GetType();
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(_values[_currentReadResult][_currentRow][i]);
        }

        public Guid GetGuid(int i)
        {
            return Guid.Parse(Convert.ToString(_values[_currentReadResult][_currentRow][i]));
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(_values[_currentReadResult][_currentRow][i]);
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(_values[_currentReadResult][_currentRow][i]);
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(_values[_currentReadResult][_currentRow][i]);
        }

        public string GetName(int i)
        {
            return _columnNames[_currentReadResult][i];
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _columnNames[_currentReadResult].Length; i++)
            {
                if (name == _columnNames[_currentReadResult][i])
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return Convert.ToString(_values[_currentReadResult][_currentRow][i]);
        }

        public object GetValue(int i)
        {
            return _values[_currentReadResult][_currentRow][i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return _values[_currentReadResult][_currentRow][i] == null;
        }

        public bool NextResult()
        {
            if (_currentReadResult < _columnNames.Count - 1)
            {
                _currentReadResult++;
                _currentRow = -1;
                return true;
            }
            return false;
        }

        public bool Read()
        {
            if (_currentRow < _values[_currentReadResult].Count - 1)
            {
                _currentRow++;
                return true;
            }

            return false;
        }
    }
}
