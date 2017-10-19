using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class FromClause<TQueryImpl> : Clause<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        internal string TableName { get; private set; }
        internal string TableAlias { get; private set; }

        internal FromClause(TQueryImpl query, string tableName)
            : base(query, $"FROM {tableName}")
        {
            TableName = tableName;
        }

        public FromClause<TQueryImpl> As(string tableAlias)
        {
            _sb.Append($" AS [{tableAlias}] ");
            TableAlias = tableAlias;
            return this;
        }
    }
}
