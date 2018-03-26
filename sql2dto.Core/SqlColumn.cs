using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlColumn : SqlExpression
    {
        internal SqlColumn(SqlTable table, string propertyName, string columnName)
        {
            _table = table;
            _propertyName = propertyName;
            _columnName = columnName;
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.COLUMN;

        private readonly SqlTable _table;
        public SqlTable GetSqlTable() => _table;

        private string _propertyName;
        public string GetPropertyName() => _propertyName;

        private string _columnName;
        public string GetColumnName() => _columnName;
    }
}
