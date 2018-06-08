using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace sql2dto.Core
{
    public interface IDbParametersBag
    {
        bool AddDbParameterIfNotFound(DbParameter parameter);
        Dictionary<string, DbParameter> GetDbParameters();
    }
}
