using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlCastExpression : SqlExpression
    {
        internal SqlCastExpression(SqlExpression expressionToCast, string to)
        {
            _expressionToCast = expressionToCast;
            _sqlTypeString = to;
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.CAST;

        private SqlExpression _expressionToCast;
        public SqlExpression GetExpressionToCast() => _expressionToCast;

        private string _sqlTypeString;
        public string GetSqlTypeString() => _sqlTypeString;
    }
}
