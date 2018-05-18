using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlTable : SqlTabularSource
    {
        protected SqlTable(string tableSchema, string tableName, string tableAlias)
        {
            _tableSchema = tableSchema;
            _tableName = tableName;
            _tableAlias = tableAlias;
            _columnPropertyNamesToIndexes = new Dictionary<string, int>();
            _columnNamesToIndexes = new Dictionary<string, int>();
            _columns = new List<SqlColumn>();
        }

        public sealed override SqlTabularSourceType TabularType => SqlTabularSourceType.TABLE;

        private string _tableSchema;
        public string GetTableSchema() => _tableSchema;

        private string _tableName;
        public string GetTableName() => _tableName;

        private string _tableAlias;
        public override string GetAlias() => _tableAlias;

        private Dictionary<string, int> _columnPropertyNamesToIndexes;
        public SqlColumn GetColumnByPropertyName(string propertyName)
        {
            if (_columnPropertyNamesToIndexes.TryGetValue(propertyName, out int index))
            {
                return _columns[index];
            }
            throw new ArgumentOutOfRangeException(nameof(propertyName));
        }

        private Dictionary<string, int> _columnNamesToIndexes;
        public override SqlColumn GetColumn(string columnNameOrAlias)
        {
            if (_columnNamesToIndexes.TryGetValue(columnNameOrAlias, out int index))
            {
                return _columns[index];
            }
            throw new ArgumentOutOfRangeException(nameof(columnNameOrAlias));
        }

        private List<SqlColumn> _columns;
        public List<SqlColumn> ListAllColumns() => _columns.ToList();

        public SqlColumn DefineColumn(string propertyAndColumnName)
        {
            return DefineColumn(propertyAndColumnName, propertyAndColumnName);
        }

        public SqlColumn DefineColumn(string propertyName, string columnName)
        {
            var col = new SqlColumn(this, propertyName, columnName);
            _columns.Add(col);
            _columnPropertyNamesToIndexes.Add(propertyName, _columns.Count - 1);
            _columnNamesToIndexes.Add(columnName, _columns.Count - 1);
            return col;
        }
    }
}
