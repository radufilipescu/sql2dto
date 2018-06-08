using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlFunctionCallExpression : SqlExpression
    {
        internal SqlFunctionCallExpression(SqlFunctionName functionName, SqlExpression innerExpression, bool distinct, SqlFunctionOver over)
        {
            _functionName = functionName;
            _innerExpression = innerExpression;
            _isDistinct = distinct;
            _over = over;
        }

        private SqlFunctionName _functionName;
        public SqlFunctionName GetFunctionName() => _functionName;

        private SqlExpression _innerExpression;
        public SqlExpression GetInnerExpression() => _innerExpression;

        private bool _isDistinct;
        public bool GetIsDistinct() => _isDistinct;

        public SqlFunctionOver _over;
        public SqlFunctionOver GetOverClause() => _over;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.FUNCTION_CALL;
    }
}
