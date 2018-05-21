using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert DInt from S7 plc to C# int (Int32).
    /// </summary>
    public static class DInt
    {
        /// <summary>
        /// Converts a S7 DInt (4 bytes) to int (Int32)
        /// </summary>
        public static Int32 FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 4 bytes.");
            }
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }


        /// <summary>
        /// Converts a int (Int32) to S7 DInt (4 bytes)
        /// </summary>
        public static byte[] ToByteArray(Int32 value)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)((value >> 24) & 0xFF);
            bytes[1] = (byte)((value >> 16) & 0xFF);
            bytes[2] = (byte)((value >> 8) & 0xFF);
            bytes[3] = (byte)((value) & 0xFF);

            return bytes;
        }

        /// <summary>
        /// Converts an array of int (Int32) to an array of bytes
        /// </summary>
        public static byte[] ToByteArray(Int32[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (Int32 val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of S7 DInt to an array of int (Int32)
        /// </summary>
        public static Int32[] ToArray(byte[] bytes)
        {
            Int32[] values = new Int32[bytes.Length / 4];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
                values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

            return values;
        }
        

    }
}
