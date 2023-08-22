using System;
using S7.Net.Helper;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Words from S7 plc to C#.
    /// </summary>
    public static class Date
    {
        /// <summary>
        /// Minimum allowed date for the IEC date type
        /// </summary>
        public static readonly System.DateTime IecMinDate = new(year: 1990, month: 01, day: 01);
        
        /// <summary>
        /// Maximum allowed date for the IEC date type
        /// </summary>
        public static readonly System.DateTime IecMaxDate = new(year: 2168, month: 12, day: 31);
        
        /// <summary>
        /// Converts a word (2 bytes) to IEC date (<see cref="System.DateTime"/>)
        /// </summary>
        public static System.DateTime FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 2 bytes.");
            }

            var daysSinceDateStart = Word.FromByteArray(bytes);
            return IecMinDate.AddDays(daysSinceDateStart);
        }

        /// <summary>
        /// Converts a <see cref="System.DateTime"/> to word (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(System.DateTime dateTime) => Word.ToByteArray(dateTime.GetDaysSinceIecDateStart());

        /// <summary>
        /// Converts an array of <see cref="System.DateTime"/>s to an array of bytes
        /// </summary>
        public static byte[] ToByteArray(System.DateTime[] value)
        {
            var arr = new ByteArray();
            foreach (var date in value)
                arr.Add(ToByteArray(date));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of bytes to an array of <see cref="System.DateTime"/>s
        /// </summary>
        public static System.DateTime[] ToArray(byte[] bytes)
        {
            var values = new System.DateTime[bytes.Length / sizeof(ushort)];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = FromByteArray(
                    new[]
                    {
                        bytes[i], bytes[i + 1]
                    });
            }

            return values;
        }
    }
}