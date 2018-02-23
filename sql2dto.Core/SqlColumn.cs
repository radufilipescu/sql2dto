using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlColumn : SqlExpression
    {
        internal SqlColumn(SqlTable table, string columnName)
        {
            _table = table;
            _columnName = columnName;
            _columnAlias = null;
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.COLUMN;

        private readonly SqlTable _table;
        public SqlTable GetSqlTable() => _table;

        private string _columnName;
        public string GetColumnName() => _columnName;

        private string _columnAlias;
        public string GetColumnAlias() => _columnAlias;
    }
}
