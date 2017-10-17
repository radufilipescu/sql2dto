using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class WhereClause : Clause
    {
        internal WhereClause(Query query)
            : base(query, "WHERE ")
        {

        }

        internal SqlExpressionBuilder ToExpressionBuilder(string[] path)
        {
            return new SqlExpressionBuilder(_, path);
        }

        internal SqlExpressionBuilder ToExpressionBuilderSub()
        {
            return new SqlExpressionBuilder(_).Sub();
        }

        internal SqlExpressionBuilder ToExpressionBuilderSub(string[] path)
        {
            return new SqlExpressionBuilder(_).Sub(path);
        }
    }
}
