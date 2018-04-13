using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class SqlCTE : SqlTabularSource
    {
        internal SqlCTE(string cte)
        {
            _cteAlias = cte;
        }

        public override SqlTabularSourceType TabularType => SqlTabularSourceType.CTE;

        private string _cteAlias;
        public override string GetAlias() => _cteAlias;

        public override SqlColumn GetColumn(string columnNameOrAlias)
        {
            return null;
        }
    }
}
