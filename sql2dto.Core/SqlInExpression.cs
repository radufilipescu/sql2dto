using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlInExpression : SqlExpression, IGetInnerExpressionsList
    {
        public override SqlExpressionType GetExpressionType() => SqlExpressionType.IN;

        internal SqlInExpression(SqlExpression checkExpression, List<SqlExpression> expressionsList, bool isNotIn)
        {
            _expressionsList = expressionsList;
            _checkExpression = checkExpression;
            _isNotIn = isNotIn;
        }

        private SqlExpression _checkExpression;
        public SqlExpression GetCheckExpression() => _checkExpression;

        private List<SqlExpression> _expressionsList;
        public List<SqlExpression> GetInnerExpressionsList() => _expressionsList;

        private bool _isNotIn;
        public bool GetIsNotIn() => _isNotIn;
    }
}
