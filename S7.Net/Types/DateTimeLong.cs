using System;
using System.Collections.Generic;
using System.Linq;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert between <see cref="T:System.DateTime"/> and S7 representation of datetime values.
    /// Supported variant in S71200 & S71500
    /// </summary>
    public static class DateTimeLong
    {
        /// <summary>
        /// The minimum <see cref="T:System.DateTime"/> value supported by the specification.
        /// </summary>
        public static readonly System.DateTime SpecMinimumDateTime = new System.DateTime(1970, 1, 1);

        /// <summary>
        /// The maximum <see cref="T:System.DateTime"/> value supported by the specification.
        /// Min max per: https://support.industry.siemens.com/cs/mdm/109773506?c=93833257483&lc=en-WW
        /// Min .: DTL # 1970-01-01-00: 00: 00.0
        /// Max.: DTL # 2262-04-11-23: 47: 16.854775807
        /// </summary>
        public static readonly System.DateTime SpecMaximumDateTime = new System.DateTime(2262, 04, 11, 23, 47, 15, 999);

        /// <summary>
        /// Parses a <see cref="T:System.DateTime"/> value from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>A <see cref="T:System.DateTime"/> object representing the value read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
        ///   <paramref name="bytes"/> is not 8 or any value in <paramref name="bytes"/>
        ///   is outside the valid range of values.</exception>
        public static System.DateTime FromByteArray(byte[] bytes)
        {
            return FromByteArrayImpl(bytes);
        }

        /// <summary>
        /// Parses an array of <see cref="T:System.DateTime"/> values from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>An array of <see cref="T:System.DateTime"/> objects representing the values read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
        ///   <paramref name="bytes"/> is not a multiple of 8 or any value in
        ///   <paramref name="bytes"/> is outside the valid range of values.</exception>
        public static System.DateTime[] ToArray(byte[] bytes)
        {
            if (bytes.Length % 12 != 0)
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length,
                    $"Parsing an array of DateTime requires a multiple of 12 bytes of input data, input data is '{bytes.Length}' long.");

            var cnt = bytes.Length / 12;
            var result = new System.DateTime[bytes.Length / 12];

            for (var i = 0; i < cnt; i++)
                result[i] = FromByteArrayImpl(new ArraySegment<byte>(bytes, i * 12, 12));

            return result;
        }

        private static System.DateTime FromByteArrayImpl(IList<byte> bytes)
        {
            if (bytes.Count != 12)
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Count,
                    $"Parsing a DateTime requires exactly 12 bytes of input data, input data is {bytes.Count} bytes long.");

            byte[] byteArray = bytes.ToArray();

            int AssertRangeInclusive(int input, byte min, byte max, string field)
            {
                if (input < min)
                    throw new ArgumentOutOfRangeException(nameof(input), input,
                        $"Value '{input}' is lower than the minimum '{min}' allowed for {field}.");
                if (input > max)
                    throw new ArgumentOutOfRangeException(nameof(input), input,
                        $"Value '{input}' is higher than the maximum '{max}' allowed for {field}.");

                return input;
            }
            byte[] yrBytes = new byte[2];
            Array.Copy(byteArray, 0, yrBytes, 0, 2);

            var year =  BitConverter.ToInt16(yrBytes.Reverse().ToArray(), 0);
            var month = AssertRangeInclusive(Convert.ToInt16(bytes[2]), 1, 12, "month");
            var day = AssertRangeInclusive(Convert.ToInt16(bytes[3]), 1, 31, "day of month");
            var hour = AssertRangeInclusive(Convert.ToInt16(bytes[5]), 0, 23, "hour");
            var minute = AssertRangeInclusive(Convert.ToInt16(bytes[6]), 0, 59, "minute");
            var second = AssertRangeInclusive(Convert.ToInt16(bytes[7]), 0, 59, "second");

            // Parse the Milliseconds
            byte[] msBytes = new byte[4];
            Array.Copy(byteArray, 8, msBytes, 0, 4);
            var msec = BitConverter.ToInt32(msBytes.Reverse().ToArray(), 0);

            return new System.DateTime(year, month, day, hour, minute, second, msec, DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts a <see cref="T:System.DateTime"/> value to a byte array.
        /// </summary>
        /// <param name="dateTime">The DateTime value to convert.</param>
        /// <returns>A byte array containing the S7 date time representation of <paramref name="dateTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of
        ///   <paramref name="dateTime"/> is before <see cref="P:SpecMinimumDateTime"/>
        ///   or after <see cref="P:SpecMaximumDateTime"/>.</exception>
        public static byte[] ToByteArray(System.DateTime dateTime)
        {
            if (dateTime < SpecMinimumDateTime)
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime,
                    $"Date time '{dateTime}' is before the minimum '{SpecMinimumDateTime}' supported in S7 date time representation.");

            byte[] dtl = new byte[0];
                
            byte[] Year = Int.ToByteArray((short) dateTime.Year);
            byte Month = Convert.ToByte(Int.CWord(dateTime.Month));
            byte Day = Convert.ToByte(Int.CWord(dateTime.Day));
            byte DoW = Convert.ToByte(Int.CWord((short)dateTime.DayOfWeek+1));
            byte Hour = Convert.ToByte(Int.CWord(dateTime.Hour));
            byte Minute = Convert.ToByte(Int.CWord(dateTime.Minute));
            byte Second = Convert.ToByte(Int.CWord(dateTime.Second));
            byte[] Milliseconds = DInt.ToByteArray(dateTime.Millisecond * 1000000);
            byte[] ret = dtl.Concat(Year)
                            .Concat(AsByteArray(Month))
                            .Concat(AsByteArray(Day))
                            .Concat(AsByteArray(DoW))
                            .Concat(AsByteArray(Hour))
                            .Concat(AsByteArray(Minute))
                            .Concat(AsByteArray(Second))
                            .Concat(Milliseconds).ToArray();
            return ret;
        }

        /// <summary>
        /// Converts an array of <see cref="T:System.DateTime"/> values to a byte array.
        /// </summary>
        /// <param name="dateTimes">The DateTime values to convert.</param>
        /// <returns>A byte array containing the S7 date time long (DTL) representations of <paramref name="dateTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any value of
        ///   <paramref name="dateTimes"/> is before <see cref="P:SpecMinimumDateTime"/>
        ///   or after <see cref="P:SpecMaximumDateTime"/>.</exception>
        public static byte[] ToByteArray(System.DateTime[] dateTimes)
        {
            var bytes = new List<byte>(dateTimes.Length * 12);
            foreach (var dateTime in dateTimes) bytes.AddRange(ToByteArray(dateTime));

            return bytes.ToArray();
        }

        private static byte[] AsByteArray(byte b)
        {
            byte[] _ret = new byte[1];
            _ret[0] = b;
            return _ret;
        }
    }
}