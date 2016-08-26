using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Int from S7 plc to C#.
    /// </summary>
    public static class Int
    {
        /// <summary>
        /// Converts a S7 Int (2 bytes) to short (Int16)
        /// </summary>
        public static Int16 FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 2 bytes.");
            }
            // bytes[0] -> HighByte
            // bytes[1] -> LowByte
            return FromBytes(bytes[1], bytes[0]);
        }

        /// <summary>
        /// Converts a S7 Int (2 bytes) to short (Int16)
        /// </summary>
        public static Int16 FromBytes(byte LoVal, byte HiVal)
        {
            return (Int16)(HiVal * 256 + LoVal);
        }

        /// <summary>
        /// Converts a short (Int16) to a S7 Int byte array (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(Int16 value)
        {
            byte[] bytes = new byte[2];
            int x = 2;
            long valLong = (long)((Int16)value);
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
        /// Converts an array of short (Int16) to a S7 Int byte array (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(Int16[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (Int16 val in value)
                arr.Add(ToByteArray(val));
            return arr.array;
        }

        /// <summary>
        /// Converts an array of S7 Int to an array of short (Int16)
        /// </summary>
        public static Int16[] ToArray(byte[] bytes)
        {
            Int16[] values = new Int16[bytes.Length / 2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 2; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++] });

            return values;
        }
        
        /// <summary>
        /// Converts a C# int value to a C# short value, to be used as word.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int16 CWord(int value)
        {
            if (value > 32767)
            {
                value -= 32768;
                value = 32768 - value;
                value *= -1;
            }
            return (short)value;
        }

    }
}
