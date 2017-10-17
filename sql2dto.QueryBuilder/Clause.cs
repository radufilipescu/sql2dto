using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public abstract class Clause
    {
        protected StringBuilder _sb;

        protected Query _query;
        public Query _
        {
            get
            {
                _query.AddSqlPart(_sb.ToString());
                return _query;
            }
            private set { _query = value; }
        }

        public Clause(Query query, string init = null)
        {
            _sb = new StringBuilder($"{Environment.NewLine}{init}");
            _query = query;
        }
    }
}
