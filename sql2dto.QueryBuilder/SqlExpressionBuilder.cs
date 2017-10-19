using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SqlExpressionBuilder<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        private StringBuilder _sb;

        private TQueryImpl _query;
        public TQueryImpl _
        {
            get
            {
                _query.AddSqlPart(_sb.ToString());
                return _query;
            }
            private set { _query = value; }
        }

        internal SqlExpressionBuilder(TQueryImpl query)
        {
            _query = query;
            _sb = new StringBuilder();
        }

        internal SqlExpressionBuilder(TQueryImpl query, string[] path)
        {
            _query = query;
            _sb = new StringBuilder(QueryValidation<TQueryImpl>.JoinPath(_query, path));
        }

        public SqlExpressionBuilder<TQueryImpl> And(params string[] path)
        {
            _sb.Append(" AND ");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> AndSub()
        {
            _sb.Append(" AND (");
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> AndSub(params string[] path)
        {
            _sb.Append(" AND (");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> Or(params string[] path)
        {
            _sb.Append(" OR ");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> OrSub()
        {
            _sb.Append(" OR (");
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> OrSub(params string[] path)
        {
            _sb.Append(" OR (");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> Sub()
        {
            _sb.Append(" (");
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> Sub(params string[] path)
        {
            _sb.Append(" (");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> EndSub()
        {
            _sb.Append(") ");
            return this;
        }

        public SqlExpressionBuilder<TQueryImpl> IsEqualTo(params string[] path)
        {
            _sb.Append(" = ");
            _sb.Append(QueryValidation<TQueryImpl>.JoinPath(_query, path));
            return this;
        }
    }
}
