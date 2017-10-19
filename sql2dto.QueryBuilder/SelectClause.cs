using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SelectClause<TQueryImpl> : Clause<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        internal SelectClause(TQueryImpl query)
            : base(query, "SELECT ")
        {

        }

        internal SelectClause<TQueryImpl> All()
        {
            _sb.Append(" *");
            return this;
        }

        public SelectClause<TQueryImpl> AndAll()
        {
            _sb.Append(", *");
            return this;
        }

        internal SelectClause<TQueryImpl> Columns(string tableAlias, params string[] columnNames)
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
                if (!String.IsNullOrEmpty(_query.ColumnPrefixTableAliasFormat))
                {
                    _sb.Append($" AS {String.Format(_query.ColumnPrefixTableAliasFormat, tableAlias)}{columnName}");
                }
            }
            return this;
        }

        public SelectClause<TQueryImpl> AndColumns(string tableAlias, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                _sb.Append($", [{tableAlias}].[{columnName}]");
                if (!String.IsNullOrEmpty(_query.ColumnPrefixTableAliasFormat))
                {
                    _sb.Append($" AS {String.Format(_query.ColumnPrefixTableAliasFormat, tableAlias)}{columnName}");
                }
            }
            return this;
        }
    }
}
