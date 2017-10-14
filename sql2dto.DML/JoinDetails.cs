using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    internal enum JoinType
    {
        INNER = 0,
        LEFT = 1,
        RIGHT = 2,
        FULL = 3
    }

    internal class JoinDetails
    {
        internal JoinType JoinType { get; private set; }
        internal SqlTable JoinedTable { get; private set; }
        internal string LeftTableAlias { get; private set; }
        internal SqlColumn LeftColumn { get; private set; }
        internal SqlOp Op { get; private set; }
        internal string RightTableAlias { get; private set; }
        internal SqlColumn RightColumn { get; private set; }

        public JoinDetails(JoinType joinType, SqlTable joinedTable, string leftTableAlias, SqlColumn leftColumn, SqlOp op, string rightTableAlias, SqlColumn)
        {
            JoinType = JoinType;
            JoinedTable = joinedTable;
            LeftTableAlias = leftTableAlias;
            LeftColumn = leftColumn;
            Op = op;
            RightTableAlias = rightTableAlias;
        }
    }
}
