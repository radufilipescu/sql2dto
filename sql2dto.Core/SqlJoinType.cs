using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlJoinType
    {
        NONE,
        INNER,
        LEFT,
        RIGHT,
        FULL,
        CROSS,
    }
}
