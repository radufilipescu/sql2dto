using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public interface ISelectable
    {
        string BuildSqlSelect(string parent = null);
    }
}
