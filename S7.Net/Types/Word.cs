using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Words from S7 plc to C#.
    /// </summary>
    public static class Word
    {
        /// <summary>
        /// Converts a word (2 bytes) to ushort (UInt16)
        /// </summary>
        public static UInt16 FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 2 bytes.");
            }

            return (UInt16)((bytes[0] << 8) | bytes[1]);
        }


        /// <summary>
        /// Converts 2 bytes to ushort (UInt16)
        /// </summary>
        public static UInt16 FromBytes(byte b1, byte b2)
        {
            return (UInt16)((b2 << 8) | b1);
        }


        /// <summary>
        /// Converts a ushort (UInt16) to word (2 bytes)
        /// </summary>
        public static byte[] ToByteArray(UInt16 value)
        {
            byte[] bytes = new byte[2];

            bytes[1] = (byte)(value & 0xFF);
            bytes[0] = (byte)((value>>8) & 0xFF);

            return bytes;
        }

        /// <summary>
        /// Converts an array of ushort (UInt16) to an array of bytes
        /// </summary>
        public static byte[] ToByteArray(UInt16[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt16 val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of bytes to an array of ushort
        /// </summary>
        public static UInt16[] ToArray(byte[] bytes)
        {
            UInt16[] values = new UInt16[bytes.Length/2];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length/2; cnt++)
                values[cnt] = FromByteArray(new byte[] {bytes[counter++], bytes[counter++]});

            return values;
        }
    }
}
