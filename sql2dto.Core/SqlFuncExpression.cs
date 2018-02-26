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
            return new SqlFuncExpression(SqlFunctionName.SUM, expression, distinct: false);
        }

        public static SqlFuncExpression SumDistinct(SqlExpression expression)
        {
            return new SqlFuncExpression(SqlFunctionName.SUM, expression, distinct: true);
        }
    }

    public class SqlFuncExpression : SqlExpression
    {
        internal SqlFuncExpression(SqlFunctionName functionName, SqlExpression innerExpression, bool distinct)
        {
            FunctionName = functionName;
            InnerExpression = innerExpression;
            IsDistinct = distinct;
        }

        public SqlFunctionName FunctionName { get; private set; }
        public SqlExpression InnerExpression { get; private set; }
        public bool IsDistinct { get; private set; }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.FUNCTION_CALL;
    }
}
