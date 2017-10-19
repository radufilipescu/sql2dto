using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class WhereClause<TQueryImpl> : Clause<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        internal WhereClause(TQueryImpl query)
            : base(query, "WHERE ")
        {

        }

        internal SqlExpressionBuilder<TQueryImpl> ToExpressionBuilder(string[] path)
        {
            return new SqlExpressionBuilder<TQueryImpl>(_, path);
        }

        internal SqlExpressionBuilder<TQueryImpl> ToExpressionBuilderSub()
        {
            return new SqlExpressionBuilder<TQueryImpl>(_).Sub();
        }

        internal SqlExpressionBuilder<TQueryImpl> ToExpressionBuilderSub(string[] path)
        {
            return new SqlExpressionBuilder<TQueryImpl>(_).Sub(path);
        }
    }
}
