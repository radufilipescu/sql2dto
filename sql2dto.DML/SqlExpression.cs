using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public enum SqlOp
    {
        Equals = 1000,
        NotEquals = 1001,

        GreaterThan = 2000,
        GreaterOrEqualTo = 2001,
        LowerThan = 2002,
        LowerOrEqualTo = 2003
    }

    public abstract class SqlExpression
    {
        internal abstract string BuildSqlSelect(string leftParent = null, string rightParent = null);

        public static string GetSqlOperator(SqlOp op)
        {
            switch (op)
            {
                case SqlOp.Equals:
                    return "=";
                case SqlOp.NotEquals:
                    return "!=";
                case SqlOp.GreaterThan:
                    return ">";
                case SqlOp.GreaterOrEqualTo:
                    return ">=";
                case SqlOp.LowerThan:
                    return "<";
                case SqlOp.LowerOrEqualTo:
                    return "<=";
            }
            throw new NotImplementedException();
        }
    }

    public class SqlExpression<TLeft, TRight> : SqlExpression
        where TLeft : ISelectable
        where TRight : ISelectable
    {
        public TLeft Left { get; private set; }
        public SqlOp Op { get; private set; }
        public TRight Right { get; private set; }

        internal SqlExpression(TLeft left, SqlOp op, TRight right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        internal override string BuildSqlSelect(string leftParent = null, string rightParent = null)
        {
            return $"{Left.BuildSqlSelect(leftParent)} {SqlExpression.GetSqlOperator(Op)} {Right.BuildSqlSelect(rightParent)}";
        }
    }
}
