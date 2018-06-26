using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public static SqlConstantExpression Const(bool value)
        {
            var constExpression = new SqlConstantExpression(SqlConstantType.BOOLEAN, value.ToString());
            constExpression.BooleanValue = value;
            return constExpression;
        }
        #endregion

        #region IS NULL
        public static SqlIsNullExpression IsNull(SqlExpression expression)
        {
            return new SqlIsNullExpression(expression);
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

        public static SqlCaseWhenExpression Case(bool constant)
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

        #region non windowing functions
        public static SqlFunctionCallExpression Concat(params SqlExpression[] expressions)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.CONCAT, expressions.ToList(), distinct: false, over: null);
        }
        #endregion

        #region windowing functions
        public static SqlFunctionCallExpression FuncCall(string funcName, SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(funcName, new List<SqlExpression> { expression }, distinct: false, over: over);
        }

        public static SqlFunctionCallExpression FuncCallDistinct(string funcName, SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(funcName, new List<SqlExpression> { expression }, distinct: true, over: over);
        }

        public static SqlFunctionOver Over()
        {
            return new SqlFunctionOver();
        }

        public static SqlFunctionCallExpression Sum(SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.SUM, new List<SqlExpression> { expression }, distinct: false, over: over);
        }

        public static SqlFunctionCallExpression SumDistinct(SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.SUM, new List<SqlExpression> { expression }, distinct: true, over: over);
        }

        public static SqlFunctionCallExpression Avg(SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.AVERAGE, new List<SqlExpression> { expression }, distinct: false, over: over);
        }

        public static SqlFunctionCallExpression AvgDistinct(SqlExpression expression, SqlFunctionOver over = null)
        {
            return new SqlFunctionCallExpression(SqlFunctionName.AVERAGE, new List<SqlExpression> { expression }, distinct: true, over: over);
        }
        #endregion

        #endregion

        #region CTE
        public static SqlCTE CTE(string cte)
        {
            return new SqlCTE(cte);
        }

        public static SqlColumn CTEColumn(string cte, string columnName)
        {
            return new SqlColumn(new SqlCTE(cte), null, columnName);
        }
        #endregion

        #region CAST
        public static SqlCastExpression Cast(SqlExpression expressionToCast, string to)
        {
            return new SqlCastExpression(expressionToCast, to);
        }
        #endregion

        #region TUPLE
        public static SqlTupleExpression Tuple(params SqlExpression[] items)
        {
            return new SqlTupleExpression(items);
        }
        #endregion

        #region NOT
        public static SqlExpression Not(SqlExpression innerExpression)
        {
            return new SqlNotExpression(innerExpression);
        }
        #endregion

        #region EXISTS
        public static SqlExpression Exists(params SqlExpression[] innerExpressions)
        {
            return new SqlExistsExpression(innerExpressions.ToList());
        }

        public static SqlExpression NotExists(params SqlExpression[] innerExpressions)
        {
            return new SqlNotExpression(new SqlExistsExpression(innerExpressions.ToList()));
        }
        #endregion

        #region ANY
        public static SqlExpression Any(params SqlExpression[] innerExpressions)
        {
            return new SqlAnyExpression(innerExpressions.ToList());
        }
        #endregion

        #region ALL
        public static SqlExpression All(params SqlExpression[] innerExpressions)
        {
            return new SqlAllExpression(innerExpressions.ToList());
        }
        #endregion
    }
}
