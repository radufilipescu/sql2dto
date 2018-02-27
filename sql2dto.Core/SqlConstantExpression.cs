using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlConstantExpression : SqlExpression
    {
        internal SqlConstantExpression(SqlConstantType constantType, string value)
        {
            _constantType = constantType;
            _value = value;
        }

        private string _value;
        public string GetValue() => _value;

        private SqlConstantType _constantType;
        public SqlConstantType GetConstantType() => _constantType;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.CONSTANT;
    }
}
