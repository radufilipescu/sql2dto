using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public abstract class BooleanTranslator
    {
        public abstract string BooleanTrueStringExpression { get; }
        public abstract string BooleanFalseStringExpression { get; }

        public Func<short, bool> FromInt16ToBool { get; set; }
        public Func<int, bool> FromInt32ToBool { get; set; }
        public Func<long, bool> FromInt64ToBool { get; set; }

        public Func<double, bool> FromDoubleToBool { get; set; }
        public Func<float, bool> FromFloatToBool { get; set; }
        public Func<decimal, bool> FromDecimalToBool { get; set; }

        public Func<char, bool> FromCharToBool { get; set; }
        public Func<string, bool> FromStringToBool { get; set; }
    }
}
