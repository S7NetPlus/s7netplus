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
        public static short FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 2 bytes.");
            }
            // bytes[0] -> HighByte
            // bytes[1] -> LowByte
            return (short)((int)(bytes[1]) | ((int)(bytes[0]) << 8));
        }


        /// <summary>
        /// Converts a short (Int16) to a S7 Int byte array (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(Int16 value)
        {
            byte[] bytes = new byte[2];

            bytes[0] = (byte) (value >> 8 & 0xFF);
            bytes[1] = (byte)(value & 0xFF);

            return bytes;
        }

        /// <summary>
        /// Converts an array of short (Int16) to a S7 Int byte array (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(Int16[] value)
        {
            byte[] bytes = new byte[value.Length * 2];
            int bytesPos = 0;

            for(int i=0; i< value.Length; i++)
            {
                bytes[bytesPos++] = (byte)((value[i] >> 8) & 0xFF);
                bytes[bytesPos++] = (byte) (value[i] & 0xFF);
            }
            return bytes;
        }

        /// <summary>
        /// Converts an array of S7 Int to an array of short (Int16)
        /// </summary>
        public static Int16[] ToArray(byte[] bytes)
        {
            int shortsCount = bytes.Length / 2;

            Int16[] values = new Int16[shortsCount];

            int counter = 0;
            for (int cnt = 0; cnt < shortsCount; cnt++)
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
