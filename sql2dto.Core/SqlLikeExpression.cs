using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlLikeExpression : SqlExpression
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.LIKE;

        internal SqlLikeExpression(SqlExpression inputExpression, SqlExpression patternExpression)
        {
            _inputExpression = inputExpression;
            _patternExpression = patternExpression;
        }

        private SqlExpression _inputExpression;
        public SqlExpression GetInputExpression() => _inputExpression;

        private SqlExpression _patternExpression;
        public SqlExpression GetPatternExpression() => _patternExpression;
    }
}
