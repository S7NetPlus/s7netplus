using System;
using System.Collections.Generic;
using System.IO;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert between <see cref="T:System.DateTime" /> and S7 representation of DateTimeLong (DTL) values.
    /// </summary>
    public static class DateTimeLong
    {
        public const int TypeLengthInBytes = 12;
        /// <summary>
        /// The minimum <see cref="T:System.DateTime" /> value supported by the specification.
        /// </summary>
        public static readonly System.DateTime SpecMinimumDateTime = new System.DateTime(1970, 1, 1);

        /// <summary>
        /// The maximum <see cref="T:System.DateTime" /> value supported by the specification.
        /// </summary>
        public static readonly System.DateTime SpecMaximumDateTime = new System.DateTime(2262, 4, 11, 23, 47, 16, 854);

        /// <summary>
        /// Parses a <see cref="T:System.DateTime" /> value from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>A <see cref="T:System.DateTime" /> object representing the value read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the length of
        /// <paramref name="bytes" /> is not 12 or any value in <paramref name="bytes" />
        /// is outside the valid range of values.
        /// </exception>
        public static System.DateTime FromByteArray(byte[] bytes)
        {
            return FromByteArrayImpl(bytes);
        }

        /// <summary>
        /// Parses an array of <see cref="T:System.DateTime" /> values from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>An array of <see cref="T:System.DateTime" /> objects representing the values read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the length of
        /// <paramref name="bytes" /> is not a multiple of 12 or any value in
        /// <paramref name="bytes" /> is outside the valid range of values.
        /// </exception>
        public static System.DateTime[] ToArray(byte[] bytes)
        {
            if (bytes.Length % TypeLengthInBytes != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length,
                    $"Parsing an array of DateTimeLong requires a multiple of 12 bytes of input data, input data is '{bytes.Length}' long.");
            }

            var cnt = bytes.Length / TypeLengthInBytes;
            var result = new System.DateTime[cnt];

            for (var i = 0; i < cnt; i++)
            {
                var slice = new byte[TypeLengthInBytes];
                Array.Copy(bytes, i * TypeLengthInBytes, slice, 0, TypeLengthInBytes);
                result[i] = FromByteArrayImpl(slice);
            }

            return result;
        }

        private static System.DateTime FromByteArrayImpl(byte[] bytes)
        {
            if (bytes.Length != TypeLengthInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length,
                    $"Parsing a DateTimeLong requires exactly 12 bytes of input data, input data is {bytes.Length} bytes long.");
            }


            var year = AssertRangeInclusive(Word.FromBytes(bytes[1], bytes[0]), 1970, 2262, "year");
            var month = AssertRangeInclusive(bytes[2], 1, 12, "month");
            var day = AssertRangeInclusive(bytes[3], 1, 31, "day of month");
            var dayOfWeek = AssertRangeInclusive(bytes[4], 1, 7, "day of week");
            var hour = AssertRangeInclusive(bytes[5], 0, 23, "hour");
            var minute = AssertRangeInclusive(bytes[6], 0, 59, "minute");
            var second = AssertRangeInclusive(bytes[7], 0, 59, "second");
            ;

            var nanoseconds = AssertRangeInclusive<uint>(DWord.FromBytes(bytes[11], bytes[10], bytes[9], bytes[8]), 0,
                999999999, "nanoseconds");

            var time = new System.DateTime(year, month, day, hour, minute, second);
            return time.AddTicks(nanoseconds / 100);
        }

        /// <summary>
        /// Converts a <see cref="T:System.DateTime" /> value to a byte array.
        /// </summary>
        /// <param name="dateTime">The DateTime value to convert.</param>
        /// <returns>A byte array containing the S7 DateTimeLong representation of <paramref name="dateTime" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value of
        /// <paramref name="dateTime" /> is before <see cref="P:SpecMinimumDateTime" />
        /// or after <see cref="P:SpecMaximumDateTime" />.
        /// </exception>
        public static byte[] ToByteArray(System.DateTime dateTime)
        {
            if (dateTime < SpecMinimumDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime,
                    $"Date time '{dateTime}' is before the minimum '{SpecMinimumDateTime}' supported in S7 DateTimeLong representation.");
            }

            if (dateTime > SpecMaximumDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime,
                    $"Date time '{dateTime}' is after the maximum '{SpecMaximumDateTime}' supported in S7 DateTimeLong representation.");
            }

            var stream = new MemoryStream(TypeLengthInBytes);
            // Convert Year
            stream.Write(Word.ToByteArray(Convert.ToUInt16(dateTime.Year)), 0, 2);

            // Convert Month
            stream.WriteByte(Convert.ToByte(dateTime.Month));

            // Convert Day
            stream.WriteByte(Convert.ToByte(dateTime.Day));

            // Convert WeekDay. NET DateTime starts with Sunday = 0, while S7DT has Sunday = 1.
            stream.WriteByte(Convert.ToByte(dateTime.DayOfWeek + 1));

            // Convert Hour
            stream.WriteByte(Convert.ToByte(dateTime.Hour));

            // Convert Minutes
            stream.WriteByte(Convert.ToByte(dateTime.Minute));

            // Convert Seconds
            stream.WriteByte(Convert.ToByte(dateTime.Second));

            // Convert Nanoseconds. Net DateTime has a representation of 1 Tick = 100ns.
            // Thus First take the ticks Mod 1 Second (1s = 10'000'000 ticks), and then Convert to nanoseconds.
            stream.Write(DWord.ToByteArray(Convert.ToUInt32(dateTime.Ticks % 10000000 * 100)), 0, 4);

            return stream.ToArray();
        }

        /// <summary>
        /// Converts an array of <see cref="T:System.DateTime" /> values to a byte array.
        /// </summary>
        /// <param name="dateTimes">The DateTime values to convert.</param>
        /// <returns>A byte array containing the S7 DateTimeLong representations of <paramref name="dateTimes" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any value of
        /// <paramref name="dateTimes" /> is before <see cref="P:SpecMinimumDateTime" />
        /// or after <see cref="P:SpecMaximumDateTime" />.
        /// </exception>
        public static byte[] ToByteArray(System.DateTime[] dateTimes)
        {
            var bytes = new List<byte>(dateTimes.Length * TypeLengthInBytes);
            foreach (var dateTime in dateTimes)
            {
                bytes.AddRange(ToByteArray(dateTime));
            }

            return bytes.ToArray();
        }

        private static T AssertRangeInclusive<T>(T input, T min, T max, string field) where T : IComparable<T>
        {
            if (input.CompareTo(min) < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input,
                    $"Value '{input}' is lower than the minimum '{min}' allowed for {field}.");
            }

            if (input.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input,
                    $"Value '{input}' is higher than the maximum '{max}' allowed for {field}.");
            }

            return input;
        }
    }
}