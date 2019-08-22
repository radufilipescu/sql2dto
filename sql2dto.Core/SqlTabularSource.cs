using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public abstract class SqlTabularSource : SqlExpression
    {
        public abstract SqlTabularSourceType TabularType { get; }
        public abstract string GetAlias();
        public abstract SqlColumn GetColumn(string columnNameOrAlias);
        public abstract bool TryGetColumn(string columnNameOrAlias, out SqlColumn sqlColumn);
    }
}
