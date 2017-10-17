using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public class SqlExpressionBuilder
    {
        private StringBuilder _sb;

        private Query _query;
        public Query _
        {
            get
            {
                _query.AddSqlPart(_sb.ToString());
                return _query;
            }
            private set { _query = value; }
        }

        internal SqlExpressionBuilder(Query query)
        {
            _query = query;
            _sb = new StringBuilder();
        }

        internal SqlExpressionBuilder(Query query, string[] path)
        {
            _query = query;
            _sb = new StringBuilder(QueryValidation.JoinPath(_query, path));
        }

        public SqlExpressionBuilder And(params string[] path)
        {
            _sb.Append(" AND ");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder AndSub()
        {
            _sb.Append(" AND (");
            return this;
        }

        public SqlExpressionBuilder AndSub(params string[] path)
        {
            _sb.Append(" AND (");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder Or(params string[] path)
        {
            _sb.Append(" OR ");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder OrSub()
        {
            _sb.Append(" OR (");
            return this;
        }

        public SqlExpressionBuilder OrSub(params string[] path)
        {
            _sb.Append(" OR (");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder Sub()
        {
            _sb.Append(" (");
            return this;
        }

        public SqlExpressionBuilder Sub(params string[] path)
        {
            _sb.Append(" (");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }

        public SqlExpressionBuilder EndSub()
        {
            _sb.Append(") ");
            return this;
        }

        public SqlExpressionBuilder IsEqualTo(params string[] path)
        {
            _sb.Append(" = ");
            _sb.Append(QueryValidation.JoinPath(_query, path));
            return this;
        }
    }
}
