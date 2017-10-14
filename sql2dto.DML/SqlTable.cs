using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SqlTable
    {
        internal string TableName { get; private set; }
        internal string TableSchema { get; private set; }

        public List<SqlColumn> _columns;

        public SqlTable(string schema, string name)
        {
            TableName = name;
            TableSchema = schema;

            _columns = new List<SqlColumn>();
        }

        public SqlColumn NewColumn(string name)
        {
            var result = new SqlColumn();
            result.ColumnName = name;
            result.ColumnTable = this;
            return result;
        }
    }
}
