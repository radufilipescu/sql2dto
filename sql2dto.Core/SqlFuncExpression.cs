using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlFunctionName
    {
        SUM,
        AVERAGE,
    }

    public static class SqlFuncs
    {
        public static SqlFuncExpression Sum(SqlExpression expression)
        {
            return new SqlFuncExpression(SqlFunctionName.SUM, expression);
        }
    }

    public class SqlFuncExpression : SqlExpression
    {
        public SqlFuncExpression(SqlFunctionName functionName, SqlExpression innerExpression)
        {
            _functionName = functionName;
            _innerExpression = innerExpression;
        }

        private SqlFunctionName _functionName;
        private SqlExpression _innerExpression;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.FUNCTION;
    }
}
