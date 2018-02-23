using System;
using System.Collections.Generic;
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
            _columnNamesToIndexes = new Dictionary<string, int>();
            _columns = new List<SqlColumn>();
        }

        public sealed override SqlTabularSourceType TabularType => SqlTabularSourceType.TABLE;

        private string _tableSchema;
        public string GetTableSchema() => _tableSchema;

        private string _tableName;
        public string GetTableName() => _tableName;

        private string _tableAlias;
        public string GetTableAlias() => _tableAlias;

        private Dictionary<string, int> _columnNamesToIndexes;

        private List<SqlColumn> _columns;

        public SqlColumn DefineColumn(string columnName)
        {
            var col = new SqlColumn(this, columnName);
            _columns.Add(col);
            _columnNamesToIndexes.Add(columnName, _columns.Count - 1);
            return col;
        }
    }
}
