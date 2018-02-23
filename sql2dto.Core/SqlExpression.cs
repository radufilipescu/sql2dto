﻿using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlExpression
    {
        public abstract SqlExpressionType GetExpressionType();

        public static SqlExpression operator ==(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.EQUALS, b);
            return result;
        }

        public static SqlExpression operator !=(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.NOT_EQUALS, b);
            return result;
        }

        public static SqlExpression operator &(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.AND, b);
            return result;
        }

        public static SqlExpression operator |(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.OR, b);
            return result;
        }
    }
}
