﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace sql2dto.Core
{
    public static class Sql
    {
        #region CONST
        public static SqlConstantExpression Const(int value)
        {
            return new SqlConstantExpression(SqlConstantType.NUMBER, value.ToString());
        }

        public static SqlConstantExpression Const(double value)
        {
            return new SqlConstantExpression(SqlConstantType.NUMBER, value.ToString());
        }

        public static SqlConstantExpression Const(float value)
        {
            return new SqlConstantExpression(SqlConstantType.NUMBER, value.ToString());
        }

        public static SqlConstantExpression Const(decimal value)
        {
            return new SqlConstantExpression(SqlConstantType.NUMBER, value.ToString());
        }

        public static SqlConstantExpression Const(string value)
        {
            return new SqlConstantExpression(SqlConstantType.STRING, value);
        }
        #endregion

        #region CASE
        public static SqlCaseWhenExpression Case(SqlExpression expression = null)
        {
            return new SqlCaseWhenExpression(expression);
        }

        public static SqlCaseWhenExpression Case(int constant)
        {
            return new SqlCaseWhenExpression(Const(constant));
        }

        public static SqlCaseWhenExpression Case(double constant)
        {
            return new SqlCaseWhenExpression(Const(constant));
        }

        public static SqlCaseWhenExpression Case(float constant)
        {
            return new SqlCaseWhenExpression(Const(constant));
        }

        public static SqlCaseWhenExpression Case(decimal constant)
        {
            return new SqlCaseWhenExpression(Const(constant));
        }

        public static SqlCaseWhenExpression Case(string constant)
        {
            return new SqlCaseWhenExpression(Const(constant));
        }
        #endregion

        #region LIKE
        public static SqlLikeExpression Like(SqlExpression inputExpression, string pattern)
        {
            return new SqlLikeExpression(inputExpression, Sql.Const(pattern));
        }

        public static SqlLikeExpression Like(SqlExpression inputExpression, SqlExpression patternExpression)
        {
            return new SqlLikeExpression(inputExpression, patternExpression);
        }
        #endregion

        #region FUNCTIONS
        public static SqlFunctionCallExpression Sum(SqlExpression expression)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.SUM, expression, distinct: false);
        }

        public static SqlFunctionCallExpression SumDistinct(SqlExpression expression)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.SUM, expression, distinct: true);
        }

        public static SqlFunctionCallExpression Avg(SqlExpression expression)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.AVERAGE, expression, distinct: false);
        }

        public static SqlFunctionCallExpression AvgDistinct(SqlExpression expression)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.AVERAGE, expression, distinct: true);
        }
        #endregion
    }
}
