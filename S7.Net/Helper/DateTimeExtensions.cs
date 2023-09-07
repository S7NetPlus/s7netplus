using System;
using S7.Net.Types;
using DateTime = System.DateTime;

namespace S7.Net.Helper
{
    public static class DateTimeExtensions
    {
        public static ushort GetDaysSinceIecDateStart(this DateTime dateTime)
        {
            if (dateTime < Date.IecMinDate)
            {
                throw new ArgumentOutOfRangeException($"DateTime must be at least {Date.IecMinDate:d}");
            }
            if (dateTime > Date.IecMaxDate)
            {
                throw new ArgumentOutOfRangeException($"DateTime must be lower than {Date.IecMaxDate:d}");
            }

            return (ushort)(dateTime - Date.IecMinDate).TotalDays;
        }
    }
}