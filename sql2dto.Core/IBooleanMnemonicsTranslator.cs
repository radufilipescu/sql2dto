using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public interface IBooleanMnemonicsTranslator
    {
        string BuildBooleanMnemonicString(bool value);

        bool FromBooleanMnemonicToBoolean(int value);
        bool? FromBooleanMnemonicToNullableBoolean(int? value);

        bool FromBooleanMnemonicToBoolean(double value);
        bool? FromBooleanMnemonicToNullableBoolean(double? value);

        bool FromBooleanMnemonicToBoolean(float value);
        bool? FromBooleanMnemonicToNullableBoolean(float? value);

        bool FromBooleanMnemonicToBoolean(decimal value);
        bool? FromBooleanMnemonicToNullableBoolean(decimal? value);

        bool FromBooleanMnemonicToBoolean(string value);
        bool? FromBooleanMnemonicToNullableBoolean(string value);
    }
}
