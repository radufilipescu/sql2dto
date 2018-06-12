using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public enum SqlOperator
    {
        #region  COMPARISION
        EQUALS,
        NOT_EQUALS,
        LESS_THAN,
        GREATER_THAN,
        LESS_OR_EQUAL_THAN,
        GREATER_OR_EQUAL_THAN,
        #endregion

        #region ARITHMETIC
        PLUS,
        MINUS,
        TIMES,
        DIVIDE,
        MOD,
        #endregion

        #region LOGICAL
        AND,
        OR,

        //BETWEEN, // (a <= this) & (this <= b)
        //NOT_BETWEEN, // (this <= a) | (this >= b)

        // special
        LIKE,
        NOT_LIKE,
        #endregion
    }
}
