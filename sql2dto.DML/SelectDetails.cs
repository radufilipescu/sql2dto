using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    internal class SelectDetails
    {
        internal string ColumnPrefix { get; set; }
        internal SqlColumn Column { get; set; }
        internal string ColumnAlias { get; set; }

        public SelectDetails(string columnPrefix, SqlColumn column, string columnAlias)
        {
            ColumnPrefix = columnPrefix;
            Column = column;
            ColumnAlias = columnAlias;
        }
    }
}
