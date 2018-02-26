using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlExpressionType
    {
        COLUMN,
        FUNCTION_CALL,
        UNARY,
        BINARY,
    }
}
