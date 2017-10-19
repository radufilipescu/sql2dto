using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public abstract class Clause<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        protected StringBuilder _sb;

        protected TQueryImpl _query;
        public TQueryImpl _
        {
            get
            {
                _query.AddSqlPart(_sb.ToString());
                return _query;
            }
            private set { _query = value; }
        }

        public Clause(TQueryImpl query, string init = null)
        {
            _sb = new StringBuilder($"{Environment.NewLine}{init}");
            _query = query;
        }
    }
}
