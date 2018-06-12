using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlStatementType
    {
        SELECT,
        INSERT,
        UPDATE,
        DELETE,
    }

    public interface ISqlStatement
    {
        SqlStatementType StatementType { get; }

        bool AddDbParameterIfNotFound(DbParameter parameter);
        Dictionary<string, DbParameter> GetDbParameters();
    }
}
