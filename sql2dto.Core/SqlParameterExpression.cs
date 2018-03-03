using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace sql2dto.Core
{
    public class SqlParameterExpression : SqlExpression
    {
        public SqlParameterExpression(DbParameter dbParameter)
        {
            _dbParameter = dbParameter;
        }

        private DbParameter _dbParameter;
        public DbParameter GetDbParameter() => _dbParameter;

        public override SqlExpressionType GetExpressionType() => SqlExpressionType.PARAMETER;
    }
}
