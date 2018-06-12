using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlNotExpression : SqlExpression
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.NOT;

        internal SqlNotExpression(SqlExpression innerExpression)
        {
            _innerExpression = innerExpression;
        }

        private SqlExpression _innerExpression;
        public SqlExpression GetInnerExpression() => _innerExpression;
    }
}
