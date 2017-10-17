using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class JoinClause : Clause
    {
        internal string TableName { get; private set; }
        internal string TableAlias { get; private set; }

        internal JoinClause(Query query, string tableName)
            : base(query, $"JOIN {tableName}")
        {
            TableName = tableName;
        }

        public JoinClause As(string tableAlias)
        {
            _sb.Append($" AS [{tableAlias}] ");
            TableAlias = tableAlias;
            return this;
        }

        public Query On(Query subQuery)
        {
            subQuery.Parent = _;
            return subQuery;
        }

        public SqlExpressionBuilder On(params string[] path)
        {
            _sb.Append($" ON ");
            return new SqlExpressionBuilder(_, path);
        }
    }
}
