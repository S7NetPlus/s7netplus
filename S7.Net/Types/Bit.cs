using System;
using System.Collections;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the conversion methods to convert Bit from S7 plc to C#.
    /// </summary>
    public static class Bit
    {
        /// <summary>
        /// Converts a Bit to bool
        /// </summary>
        public static bool FromByte(byte v, byte bitAdr)
        {
            BitArray bitArr = new BitArray(new byte[] { v });
            return bitArr[bitAdr];
        }

        /// <summary>
        /// Converts an array of bytes to a BitArray
        /// </summary>
        public static BitArray ToBitArray(byte[] bytes)
        {
            BitArray bitArr = new BitArray(bytes);
            return bitArr;
        }
    }
}
