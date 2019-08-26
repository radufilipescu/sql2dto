using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlFunctionName
    {
        NONE,

        CONCAT,

        COALESCE,

        // windowing functions
        SUM,
        AVERAGE,
    }
}
