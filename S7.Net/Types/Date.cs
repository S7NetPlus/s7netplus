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
        public static System.DateTime IecMinDate { get; } = new(year: 1990, month: 01, day: 01);
        
        /// <summary>
        /// Maximum allowed date for the IEC date type
        /// <remarks>
        /// Although the spec allows only a max date of 31-12-2168, the PLC IEC date goes up to 06-06-2169 (which is the actual
        /// WORD max value - 65535) 
        /// </remarks>
        /// </summary>
        public static System.DateTime IecMaxDate { get; } = new(year: 2169, month: 06, day: 06);
        
        private static readonly ushort MaxNumberOfDays = (ushort)(IecMaxDate - IecMinDate).TotalDays;
        
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
            if (daysSinceDateStart > MaxNumberOfDays)
            {
                throw new ArgumentException($"Read number exceeded the number of maximum days in the IEC date (read: {daysSinceDateStart}, max: {MaxNumberOfDays})", 
                    nameof(bytes));
            }
            
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
