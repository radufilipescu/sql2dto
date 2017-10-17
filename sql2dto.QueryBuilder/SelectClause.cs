using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SelectClause : Clause
    {
        internal SelectClause(Query query)
            : base(query, "SELECT ")
        {

        }

        internal SelectClause All()
        {
            _sb.Append(" *");
            return this;
        }

        public SelectClause AndAll()
        {
            _sb.Append(", *");
            return this;
        }

        internal SelectClause Columns(string tableAlias, params string[] columnNames)
        {
            bool isFirst = false;
            foreach (string columnName in columnNames)
            {
                if (!isFirst)
                {
                    isFirst = true;
                    _sb.Append(" ");
                }
                else
                {
                    _sb.Append(", ");
                }
                _sb.Append($"[{tableAlias}].[{columnName}]");
            }
            return this;
        }

        public SelectClause AndColumns(string tableAlias, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                _sb.Append($", [{tableAlias}].[{columnName}]");
            }
            return this;
        }
    }
}
