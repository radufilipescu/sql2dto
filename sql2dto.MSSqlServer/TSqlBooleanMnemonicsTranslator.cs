using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.MSSqlServer
{
    public class TSqlBooleanMnemonicsTranslator : IBooleanMnemonicsTranslator
    {
        public string BuildBooleanMnemonicString(bool value)
        {
            return value ? "1" : "0";
        }

        public virtual bool FromBooleanMnemonicToBoolean(int value)
        {
            return value == 0 ? false : true;
        }

        public virtual bool FromBooleanMnemonicToBoolean(double value)
        {
            return value == 0 ? false : true;
        }

        public virtual bool FromBooleanMnemonicToBoolean(float value)
        {
            return value == 0 ? false : true;
        }

        public virtual bool FromBooleanMnemonicToBoolean(decimal value)
        {
            return value == 0 ? false : true;
        }

        public virtual bool FromBooleanMnemonicToBoolean(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string upperValue = value.ToUpper();
            switch (upperValue)
            {
                case "T":
                    return true;
                case "F":
                    return false;
                default:
                    return false;
            }
        }

        public virtual bool? FromBooleanMnemonicToNullableBoolean(int? value)
        {
            return value.HasValue
                ? (value.Value == 0 ? false : true)
                : false;
        }

        public virtual bool? FromBooleanMnemonicToNullableBoolean(double? value)
        {
            return value.HasValue
                ? (value.Value == 0 ? false : true)
                : false;
        }

        public virtual bool? FromBooleanMnemonicToNullableBoolean(float? value)
        {
            return value.HasValue
                ? (value.Value == 0 ? false : true)
                : false;
        }

        public virtual bool? FromBooleanMnemonicToNullableBoolean(decimal? value)
        {
            return value.HasValue
                ? (value.Value == 0 ? false : true)
                : false;
        }

        public virtual bool? FromBooleanMnemonicToNullableBoolean(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string upperValue = value.ToUpper();
            switch (upperValue)
            {
                case "T":
                    return true;
                case "F":
                    return false;
                default:
                    return null;
            }
        }
    }
}
