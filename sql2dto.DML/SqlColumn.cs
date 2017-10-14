using System;
using System.Data;

namespace sql2dto.QueryBuilder
{
    public class SqlColumn : ISelectable
    {
        internal string ColumnName { get; set; }
        internal SqlTable ColumnTable { get; set; }

        internal SqlColumn()
        {

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static SqlExpression<SqlColumn, SqlColumn> operator ==(SqlColumn left, SqlColumn right)
        {
            return new SqlExpression<SqlColumn, SqlColumn>(left, SqlOp.Equals, right);
        }

        public static SqlExpression<SqlColumn, SqlColumn> operator !=(SqlColumn left, SqlColumn right)
        {
            return new SqlExpression<SqlColumn, SqlColumn>(left, SqlOp.NotEquals, right);
        }

        public static SqlExpression<SqlColumn, SqlColumn> operator >(SqlColumn left, SqlColumn right)
        {
            return new SqlExpression<SqlColumn, SqlColumn>(left, SqlOp.GreaterThan, right);
        }

        public static SqlExpression<SqlColumn, SqlColumn> operator <(SqlColumn left, SqlColumn right)
        {
            return new SqlExpression<SqlColumn, SqlColumn>(left, SqlOp.LowerThan, right);
        }

        public string BuildSqlSelect(string parent = null)
        {
            if (String.IsNullOrWhiteSpace(parent))
            {
                return $"[{ColumnName}]";
            }
            else
            {
                return $"[{parent}].[{ColumnName}]";
            }
        }
    }
}
