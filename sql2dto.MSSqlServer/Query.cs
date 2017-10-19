using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace sql2dto.MSSqlServer
{
    public class Query : QueryBuilder.Query<Query>
    {
        internal Query()
            : base(QueryBuilder.DatabaseType.MS_SQLServer_2016)
        {

        }

        internal Query(int year)
            : base(
                  year == 2016 ? QueryBuilder.DatabaseType.MS_SQLServer_2016 : 
                  year == 2014 ? QueryBuilder.DatabaseType.MS_SQLServer_2016 : 
                  year == 2012 ? QueryBuilder.DatabaseType.MS_SQLServer_2016 : 
                  year == 2008 ? QueryBuilder.DatabaseType.MS_SQLServer_2016 : 
                  
                  QueryBuilder.DatabaseType.MS_SQLServer_2016      
            )
        {

        }

        public static QueryBuilder.SelectClause<Query> SelectAll()
        {
            var q = new Query();
            return QueryBuilder.Query<Query>.SelectAll(q);
        }

        public static QueryBuilder.SelectClause<Query> SelectColumns(string tableAlias, params string[] columnNames)
        {
            var q = new Query();
            return QueryBuilder.Query<Query>.SelectColumns(q, tableAlias, columnNames);
        }
    }
}
