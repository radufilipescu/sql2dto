using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    public enum DataType
    {
        UNKOWN,

        SMALLINT,
        INT,
        BIGINT,

        FLOAT,
        DOUBLE,
        DECIMAL,

        VARCHAR,
        NVARCHAR
    }
}
