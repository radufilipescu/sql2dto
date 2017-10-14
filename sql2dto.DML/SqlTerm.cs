using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SqlTerm
    {
        public virtual JoinClause JoinClause { get; set; }
        public bool SubStart { get; set; }
        public bool SubEnd { get; set; }

        public SqlExpr IsEqualTo(string right)
        {
            return new SqlExpr(this, Op.EQUALS, new SqlParameterTerm(right));
        }

        public SqlExpr IsNotEqualTo(string right)
        {
            return new SqlExpr(this, Op.NOT_EQUALS, new SqlParameterTerm(right));
        }

        public SqlExpr And(string right)
        {
            return new SqlExpr(this, Op.AND, new SqlParameterTerm(right));
        }

        public SqlExpr Or(string right)
        {
            return new SqlExpr(this, Op.OR, new SqlParameterTerm(right));
        }

        public SqlExpr IsEqualTo(string rightTableAlias, SqlColumn rightColumn)
        {
            return new SqlExpr(this, Op.EQUALS, new SqlColumnTerm(rightTableAlias, rightColumn));
        }

        public SqlExpr IsNotEqualTo(string rightTableAlias, SqlColumn rightColumn)
        {
            return new SqlExpr(this, Op.NOT_EQUALS, new SqlColumnTerm(rightTableAlias, rightColumn));
        }

        public SqlExpr And(string rightTableAlias, SqlColumn rightColumn)
        {
            return new SqlExpr(this, Op.AND, new SqlColumnTerm(rightTableAlias, rightColumn));
        }

        public SqlExpr Or(string rightTableAlias, SqlColumn rightColumn)
        {
            return new SqlExpr(this, Op.OR, new SqlColumnTerm(rightTableAlias, rightColumn));
        }

        public SqlTerm Sub(string left)
        {
            return new SqlParameterTerm(left)
            {
                SubStart = true
            };
        }

        public SqlTerm Sub(string leftTableAlias, SqlColumn leftColumn)
        {
            return new SqlColumnTerm(leftTableAlias, leftColumn)
            {
                SubStart = true
            };
        }
    }

    public class SqlColumnTerm : SqlTerm
    {
        public string TableAlias { get; set; }
        public SqlColumn Column { get; set; }

        public SqlColumnTerm(string tableAlias, SqlColumn column)
        {
            TableAlias = tableAlias;
            Column = column;
        }
    }

    public class SqlParameterTerm : SqlTerm
    {
        public string ParameterName { get; set; }

        public SqlParameterTerm(string parameterName)
        {
            ParameterName = parameterName;
        }
    }

    public enum Op
    {
        EQUALS = 0,
        NOT_EQUALS = 1,

        AND = 3,
        OR = 4
    }

    public class SqlExpr : SqlTerm
    {
        public Query _
        {
            get
            {
                return JoinClause.Query;
            }
        }

        public override JoinClause JoinClause
        {
            get
            {
                return Left.JoinClause;
            }
        }



        public SqlTerm Left { get; set; }
        public Op Operator { get; set; }
        public SqlTerm Right { get; set; }

        internal SqlExpr(SqlTerm left, Op op, SqlTerm right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public SqlExpr EndSub()
        {
            Right.SubEnd = true;
            return this;
        }

        public void X()
        {
            //((new SqlColumnTerm("", new SqlColumn())) == ("", new SqlColumn())).
            //((new SqlColumnTerm("", new SqlColumn())) == "panda")._.
        }
    }

    public enum SqlJoinType
    {
        INNER = 0,
        LEFT = 1,
        RIGHT = 2,
        FULL = 3
    }

    public class JoinClause
    {
        public Query Query { get; set; }
        public SqlTable JoinedTable { get; set; }
        public SqlJoinType JoinType { get; set; }

        internal JoinClause(Query query, SqlJoinType joinType, SqlTable joinedTable)
        {
            Query = query;
            JoinType = joinType;
            JoinedTable = joinedTable;
        }

        public SqlColumnTerm On(string tableAlias, SqlColumn column)
        {
            return new SqlColumnTerm(tableAlias, column)
            {
                JoinClause = this
            };
        }

        public SqlParameterTerm On(string parameterName)
        {
            return new SqlParameterTerm(parameterName)
            {
                JoinClause = this
            };
        }
    }
}
