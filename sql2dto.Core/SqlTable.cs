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
            _columnSqlNamesToIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _columnPropertyNamesToIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _columns = new List<SqlColumn>();
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.TABLE;

        public sealed override SqlTabularSourceType TabularType => SqlTabularSourceType.TABLE;

        private string _tableSchema;
        public string GetTableSchema() => _tableSchema;

        private string _tableName;
        public string GetTableName() => _tableName;

        private string _tableAlias;
        public override string GetAlias() => _tableAlias;

        private Dictionary<string, int> _columnSqlNamesToIndexes;
        private Dictionary<string, int> _columnPropertyNamesToIndexes;
        public override SqlColumn GetColumn(string columnNameOrAlias)
        {
            if (_columnSqlNamesToIndexes.TryGetValue(columnNameOrAlias, out int colIndex))
            {
                return _columns[colIndex];
            }
            if (_columnPropertyNamesToIndexes.TryGetValue(columnNameOrAlias, out int propIndex))
            {
                return _columns[propIndex];
            }
            throw new ArgumentOutOfRangeException(nameof(columnNameOrAlias));
        }

        public override bool TryGetColumn(string columnNameOrAlias, out SqlColumn sqlColumn)
        {
            if (_columnSqlNamesToIndexes.TryGetValue(columnNameOrAlias, out int colIndex))
            {
                sqlColumn = _columns[colIndex];
                return true;
            }
            if (_columnPropertyNamesToIndexes.TryGetValue(columnNameOrAlias, out int propIndex))
            {
                sqlColumn = _columns[propIndex];
                return true;
            }

            sqlColumn = null;
            return false;
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
            try
            {
                _columnSqlNamesToIndexes.Add(columnName, _columns.Count - 1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Found duplicated column SQL name '{columnName}'", ex);
            }
            try
            {
                _columnPropertyNamesToIndexes.Add(propertyName, _columns.Count - 1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Found duplicated column property name '{propertyName}'", ex);
            }
            return col;
        }
    }
}
