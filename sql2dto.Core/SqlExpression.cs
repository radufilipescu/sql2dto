using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlExpression
    {
        protected SqlExpression()
        {
            Metadata = new Dictionary<string, string>();
        }

        public readonly Dictionary<string, string> Metadata;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public abstract SqlExpressionType GetExpressionType();

        #region  COMPARISION
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

        public static SqlExpression operator <(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.LESS_THAN, b);
            return result;
        }

        public static SqlExpression operator >(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.GREATER_THAN, b);
            return result;
        }

        public static SqlExpression operator <=(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.LESS_OR_EQUAL_THAN, b);
            return result;
        }

        public static SqlExpression operator >=(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.GREATER_OR_EQUAL_THAN, b);
            return result;
        }

        public static SqlExpression operator !(SqlExpression innerExpression)
        {
            return new SqlNotExpression(innerExpression);
        }
        #endregion

        #region ARITHMETIC
        public static SqlExpression operator +(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.PLUS, b);
            return result;
        }

        public static SqlExpression operator -(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.PLUS, b);
            return result;
        }

        public static SqlExpression operator *(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.TIMES, b);
            return result;
        }

        public static SqlExpression operator /(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.DIVIDE, b);
            return result;
        }

        public static SqlExpression operator %(SqlExpression a, SqlExpression b)
        {
            var result = new SqlBinaryExpression(a, SqlOperator.MOD, b);
            return result;
        }
        #endregion

        #region LOGICAL
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

        #region BETWEEN
        public SqlExpression Between(SqlExpression a, SqlExpression b)
        {
            return (a <= this) & (this <= b);
        }

        public SqlExpression NotBetween(SqlExpression a, SqlExpression b)
        {
            return (this <= a) | (this >= b);
        }
        #endregion

        #region LIKE
        public SqlExpression Like(string pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.LIKE, Sql.Const(pattern));
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }

        public SqlExpression Like(SqlExpression pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.LIKE, pattern);
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }

        public SqlExpression Like(SqlQuery pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.LIKE, pattern);
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }

        public SqlExpression NotLike(string pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.NOT_LIKE, Sql.Const(pattern));
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }

        public SqlExpression NotLike(SqlExpression pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.NOT_LIKE, pattern);
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }

        public SqlExpression NotLike(SqlQuery pattern, string escapeChar = null)
        {
            var result = new SqlBinaryExpression(this, SqlOperator.NOT_LIKE, pattern);
            if (!String.IsNullOrEmpty(escapeChar))
            {
                result.Metadata.Add("ESCAPE", escapeChar);
            }
            return result;
        }
        #endregion

        #region IN
        public SqlExpression In(params SqlExpression[] expressions)
        {
            return new SqlInExpression(this, expressions.ToList(), isNotIn: false);
        }

        public SqlExpression NotIn(params SqlExpression[] expressions)
        {
            return new SqlInExpression(this, expressions.ToList(), isNotIn: true);
        }
        #endregion
        #endregion
    }
}
