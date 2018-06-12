using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlExistsExpression : SqlExpression, IGetInnerExpressionsList
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.EXISTS;

        internal SqlExistsExpression(List<SqlExpression> expressionsList)
        {
            _expressionsList = expressionsList;
        }

        private List<SqlExpression> _expressionsList;
        public List<SqlExpression> GetInnerExpressionsList() => _expressionsList;
    }
}
