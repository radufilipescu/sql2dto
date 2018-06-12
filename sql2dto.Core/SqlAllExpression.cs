using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlAllExpression : SqlExpression, IGetInnerExpressionsList
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.ALL;

        internal SqlAllExpression(List<SqlExpression> expressionsList)
        {
            _expressionsList = expressionsList;
        }

        private List<SqlExpression> _expressionsList;
        public List<SqlExpression> GetInnerExpressionsList() => _expressionsList;
    }
}
