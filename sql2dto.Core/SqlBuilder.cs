using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlBuilder
    {
        public abstract string BuildExpressionString(SqlExpression expression, string expressionAlias = null);
        public abstract string BuildTableAliasString(SqlTable table);
        public abstract string BuildTableAsAliasString(SqlTable table, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildQueryString(SqlQuery query);
        public abstract string BuildQueryAliasString(SqlQuery query);
        public abstract string BuildQueryAsAliasString(SqlQuery query, SqlJoinType joinType, SqlExpression condition = null);
        public abstract string BuildSqlJoinTypeString(SqlJoinType joinType);
        public abstract string BuildSqlOperatorString(SqlOperator op);
        public abstract string BuildSqlFuncNameString(SqlFunctionName func);
        public abstract string EscapeConstantValue(string value);
    }
}
