using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlBinaryExpression : SqlExpression
    {
        internal SqlBinaryExpression(SqlExpression firstTerm, SqlOperator op, SqlExpression secondTerm)
        {
            _firstTerm = firstTerm;
            _operator = op;
            _secondTerm = secondTerm;

            Metadata = new Dictionary<string, string>();
        }

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.BINARY;

        private SqlExpression _firstTerm;
        public SqlExpression GetFirstTerm() => _firstTerm;

        private SqlOperator _operator;
        public SqlOperator GetOperator() => _operator;

        private SqlExpression _secondTerm;
        public SqlExpression GetSecondTerm() => _secondTerm;

        public readonly Dictionary<string, string> Metadata;
    }
}
