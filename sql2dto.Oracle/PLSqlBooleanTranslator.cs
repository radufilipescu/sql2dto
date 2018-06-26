using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Oracle
{
    public class PLSqlBooleanTranslator : BooleanTranslator
    {
        public override string BooleanTrueStringExpression => "1";

        public override string BooleanFalseStringExpression => "0";

        public PLSqlBooleanTranslator()
        {
            FromInt16ToBool = (value) => value != 0;
            FromInt32ToBool = (value) => value != 0;
            FromInt64ToBool = (value) => value != 0;

            FromDoubleToBool = (value) => value != 0;
            FromFloatToBool = (value) => value != 0;
            FromDecimalToBool = (value) => value != 0;

            FromCharToBool = (value) =>
            {
                switch (value)
                {
                    case 'Y':
                    case 'y':
                    case 'T':
                    case 't':
                        return true;
                    case 'N':
                    case 'n':
                    case 'F':
                    case 'f':
                        return false;
                    default:
                        throw new InvalidCastException($"PLSqlBooleanTranslator could not translate '{typeof(String).FullName}' to '{typeof(Boolean).FullName}'");

                }
            };
            FromStringToBool = (value) =>
            {
                switch (value)
                {
                    case "Y":
                    case "YES":
                    case "Yes":
                    case "y":
                    case "yes":
                    case "T":
                    case "TRUE":
                    case "True":
                    case "t":
                    case "true":
                        return true;
                    case "N":
                    case "NO":
                    case "No":
                    case "n":
                    case "no":
                    case "F":
                    case "FALSE":
                    case "False":
                    case "f":
                    case "false":
                        return false;
                    default:
                        throw new InvalidCastException($"PLSqlBooleanTranslator could not translate '{typeof(String).FullName}' to '{typeof(Boolean).FullName}'");

                }
            };
        }
    }
}
