using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlCommonTable : SqlTable
    {
        protected SqlCommonTable(string tableName, string tableAlias) 
            : base(null, tableName, tableAlias)
        {
        }

        public abstract SqlQuery Query();
    }
}
