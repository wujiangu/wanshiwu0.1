﻿namespace CSProtocol
{
    using System;

    public enum COM_ADDHERO_RESULT
    {
        COM_ADDHERO_SUCC,
        COM_ADDHERO_ACNTOWNED,
        COM_ADDHERO_PLAYERCANNOTUSE,
        COM_ADDHERO_NOFREEHERO,
        COM_ADDHERO_NOTFINDCONF,
        COM_ADDHERO_HASINITHERO,
        COM_ADDHERO_NOTINITHERO,
        COM_ADDHERO_FORBIDCOUPONS,
        COM_ADDHERO_FORBIDCOIN,
        COM_ADDHERO_OTHER,
        COM_ADDHERO_FORBIDDIAMOND,
        COM_ADDHERO_FORBIDMIXPAY,
        COM_ADDHERO_COINLESS,
        COM_ADDHERO_TIME_ERROR
    }
}

