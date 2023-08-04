using System;
using System.Collections.Generic;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert between <see cref="T:System.TimeSpan"/> and S7 representation of TIME values.
    /// </summary>
    public static class TimeSpan
    {
        /// <summary>
        /// The minimum <see cref="T:System.TimeSpan"/> value supported by the specification.
        /// </summary>
        public static readonly System.TimeSpan SpecMinimumTimeSpan = System.TimeSpan.FromMilliseconds(int.MinValue);

        /// <summary>
        /// The maximum <see cref="T:System.TimeSpan"/> value supported by the specification.
        /// </summary>
        public static readonly System.TimeSpan SpecMaximumTimeSpan = System.TimeSpan.FromMilliseconds(int.MaxValue);

        /// <summary>
        /// Parses a <see cref="T:System.TimeSpan"/> value from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>A <see cref="T:System.TimeSpan"/> object representing the value read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
        ///   <paramref name="bytes"/> is not 4 or any value in <paramref name="bytes"/>
        ///   is outside the valid range of values.</exception>
        public static System.TimeSpan FromByteArray(byte[] bytes)
        {
            var milliseconds = DInt.FromByteArray(bytes);
            return System.TimeSpan.FromMilliseconds(milliseconds);
        }

        /// <summary>
        /// Parses an array of <see cref="T:System.TimeSpan"/> values from bytes.
        /// </summary>
        /// <param name="bytes">Input bytes read from PLC.</param>
        /// <returns>An array of <see cref="T:System.TimeSpan"/> objects representing the values read from PLC.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
        ///   <paramref name="bytes"/> is not a multiple of 4 or any value in
        ///   <paramref name="bytes"/> is outside the valid range of values.</exception>
        public static System.TimeSpan[] ToArray(byte[] bytes)
        {
            const int singleTimeSpanLength = 4;
            
            if (bytes.Length % singleTimeSpanLength != 0)
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length,
                    $"Parsing an array of {nameof(System.TimeSpan)} requires a multiple of {singleTimeSpanLength} bytes of input data, input data is '{bytes.Length}' long.");

            var result = new System.TimeSpan[bytes.Length / singleTimeSpanLength];

            var milliseconds = DInt.ToArray(bytes);
            for (var i = 0; i < milliseconds.Length; i++)
                result[i] = System.TimeSpan.FromMilliseconds(milliseconds[i]);

            return result;
        }

        /// <summary>
        /// Converts a <see cref="T:System.TimeSpan"/> value to a byte array.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan value to convert.</param>
        /// <returns>A byte array containing the S7 date time representation of <paramref name="timeSpan"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of
        ///   <paramref name="timeSpan"/> is before <see cref="P:SpecMinimumTimeSpan"/>
        ///   or after <see cref="P:SpecMaximumTimeSpan"/>.</exception>
        public static byte[] ToByteArray(System.TimeSpan timeSpan)
        {
            if (timeSpan < SpecMinimumTimeSpan)
                throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan,
                    $"Time span '{timeSpan}' is before the minimum '{SpecMinimumTimeSpan}' supported in S7 time representation.");

            if (timeSpan > SpecMaximumTimeSpan)
                throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan,
                    $"Time span '{timeSpan}' is after the maximum '{SpecMaximumTimeSpan}' supported in S7 time representation.");

            return DInt.ToByteArray(Convert.ToInt32(timeSpan.TotalMilliseconds));
        }

        /// <summary>
        /// Converts an array of <see cref="T:System.TimeSpan"/> values to a byte array.
        /// </summary>
        /// <param name="timeSpans">The TimeSpan values to convert.</param>
        /// <returns>A byte array containing the S7 date time representations of <paramref name="timeSpans"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any value of
        ///   <paramref name="timeSpans"/> is before <see cref="P:SpecMinimumTimeSpan"/>
        ///   or after <see cref="P:SpecMaximumTimeSpan"/>.</exception>
        public static byte[] ToByteArray(System.TimeSpan[] timeSpans)
        {
            var bytes = new List<byte>(timeSpans.Length * 4);
            foreach (var timeSpan in timeSpans) bytes.AddRange(ToByteArray(timeSpan));

            return bytes.ToArray();
        }
    }
}