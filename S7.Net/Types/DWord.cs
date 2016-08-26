using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert DWord from S7 plc to C#.
    /// </summary>
    public static class DWord
    {
        /// <summary>
        /// Converts a S7 DWord (4 bytes) to uint (UInt32)
        /// </summary>
        public static UInt32 FromByteArray(byte[] bytes)
        {
            return FromBytes(bytes[3], bytes[2], bytes[1], bytes[0]);
        }

        /// <summary>
        /// Converts a S7 DWord (4 bytes) to uint (UInt32)
        /// </summary>
        public static UInt32 FromBytes(byte v1, byte v2, byte v3, byte v4)
        {
            return (UInt32)(v1 + v2 * Math.Pow(2, 8) + v3 * Math.Pow(2, 16) + v4 * Math.Pow(2, 24));
        }

        /// <summary>
        /// Converts a uint (UInt32) to S7 DWord (4 bytes) 
        /// </summary>
        public static byte[] ToByteArray(UInt32 value)
        {
            byte[] bytes = new byte[4];
            int x = 4;
            long valLong = (long)((UInt32)value);
            for (int cnt = 0; cnt < x; cnt++)
            {
                Int64 x1 = (Int64)Math.Pow(256, (cnt));

                Int64 x3 = (Int64)(valLong / x1);
                bytes[x - cnt - 1] = (byte)(x3 & 255);
                valLong -= bytes[x - cnt - 1] * x1;
            }
            return bytes;
        }

        /// <summary>
        /// Converts an array of uint (UInt32) to an array of S7 DWord (4 bytes) 
        /// </summary>
        public static byte[] ToByteArray(UInt32[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt32 val in value)
                arr.Add(ToByteArray(val));
            return arr.array;
        }

        /// <summary>
        /// Converts an array of S7 DWord to an array of uint (UInt32)
        /// </summary>
        public static UInt32[] ToArray(byte[] bytes)
        {
            UInt32[] values = new UInt32[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

            return values;
        }
    }
}
