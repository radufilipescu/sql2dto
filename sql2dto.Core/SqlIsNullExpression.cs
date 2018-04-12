using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlIsNullExpression : SqlExpression
    {
        internal SqlIsNullExpression(SqlExpression expression)
        {
            _expression = expression;
        }

        private SqlExpression _expression;
        public SqlExpression GetInnerExpression() => _expression;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.IS_NULL;
    }
}
