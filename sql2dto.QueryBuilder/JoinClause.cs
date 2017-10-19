using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class JoinClause<TQueryImpl> : Clause<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        internal string TableName { get; private set; }
        internal string TableAlias { get; private set; }

        internal JoinClause(TQueryImpl query, string tableName)
            : base(query, $"JOIN {tableName}")
        {
            TableName = tableName;
        }

        public JoinClause<TQueryImpl> As(string tableAlias)
        {
            _sb.Append($" AS [{tableAlias}] ");
            TableAlias = tableAlias;
            return this;
        }

        public TQueryImpl On(TQueryImpl subQuery)
        {
            subQuery.Parent = _;
            return subQuery;
        }

        public SqlExpressionBuilder<TQueryImpl> On(params string[] path)
        {
            _sb.Append($" ON ");
            return new SqlExpressionBuilder<TQueryImpl>(_, path);
        }
    }
}
