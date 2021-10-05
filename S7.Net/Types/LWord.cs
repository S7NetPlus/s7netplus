using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net.Types
{
    public static class LWord
    {
        /// <summary>
        /// Converts a S7 DWord (4 bytes) to uint (UInt64)
        /// </summary>
        public static UInt64 FromByteArray(byte[] bytes)
        {
            Array.Reverse(bytes, 0, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        
        /// <summary>
        /// Converts a uint (UInt64) to S7 LWord (8 bytes) 
        /// </summary>
        public static byte[] ToByteArray(UInt64 value)
        {
            byte[] bytes = new byte[8];

            bytes[0] = (byte)((value >> 56) & 0xFF);
            bytes[1] = (byte)((value >> 48) & 0xFF);
            bytes[2] = (byte)((value >> 40) & 0xFF);
            bytes[3] = (byte)((value >> 32) & 0xFF);
            bytes[4] = (byte)((value >> 24) & 0xFF);
            bytes[5] = (byte)((value >> 16) & 0xFF);
            bytes[6] = (byte)((value >> 8) & 0xFF);
            bytes[7] = (byte)((value) & 0xFF);

            return bytes;
        }
        
        /// <summary>
        /// Converts an array of uint (UInt64) to an array of S7 LWord (8 bytes) 
        /// </summary>
        public static byte[] ToByteArray(UInt64[] value)
        {
            ByteArray arr = new ByteArray();
            foreach (UInt64 val in value)
                arr.Add(ToByteArray(val));
            return arr.Array;
        }

        /// <summary>
        /// Converts an array of S7 LWord to an array of uint (UInt64)
        /// </summary>
        public static UInt64[] ToArray(ReadOnlySpan<byte> bytes)
        {
            UInt64[] values = new UInt64[bytes.Length / 8];

            int counter = 0;
            for (int cnt = 0; cnt < bytes.Length / 8; cnt++)
                values[cnt] = FromByteArray(bytes.Slice(cnt * 8, 8).ToArray());

            return values;
            
        }
    }
}
