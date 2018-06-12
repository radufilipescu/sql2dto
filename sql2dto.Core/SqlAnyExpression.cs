using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlAnyExpression : SqlExpression, IGetInnerExpressionsList
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.ANY;

        internal SqlAnyExpression(List<SqlExpression> expressionsList)
        {
            _expressionsList = expressionsList;
        }

        private List<SqlExpression> _expressionsList;
        public List<SqlExpression> GetInnerExpressionsList() => _expressionsList;
    }
}
