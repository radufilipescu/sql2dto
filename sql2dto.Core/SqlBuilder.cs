using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlBuilder
    {
        public abstract string GetLanguageImplementation();

        public abstract string BuildExpressionString(SqlQuery query, SqlExpression expression, string expressionAlias = null);
        public abstract string BuildAliasString(SqlTabularSource table);
        public abstract string BuildCTEJoinString(SqlQuery query, SqlCTE cte, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildTableJoinString(SqlQuery query, SqlTable table, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildQueryString(SqlQuery query);
        public abstract string BuildAliasString(SqlQuery query);
        public abstract string BuildQueryAsString(SqlQuery query, SqlQuery subQuery, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildSqlJoinTypeString(SqlJoinType joinType);
        public abstract string BuildSqlOperatorString(SqlOperator op);
        public abstract string BuildSqlFuncNameString(SqlFunctionName func);
        public abstract string BuildSqlOrderByDirectionString(SqlOrderByDirection direction);
        public abstract string EscapeConstantValue(string value);
        public abstract DbCommand BuildDbCommand(SqlQuery query);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection);
        public abstract DbCommand BuildDbCommand(SqlQuery query, DbConnection connection, DbTransaction transaction);
        public abstract SqlParameterExpression Parameter(string name, object value);
    }
}
