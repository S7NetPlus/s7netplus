using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from bytes to byte arrays
    /// </summary>
    public static class sByte
    {
        /// <summary>
        /// Converts a byte to byte array
        /// </summary>
        public static sbyte[] ToByteArray(sbyte value)
        {
            return new sbyte[] { value }; ;
        }
       
        /// <summary>
        /// Converts a byte array to byte
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static sbyte FromByteArray(sbyte[] sbytes)
        {
            if (sbytes.Length != 1)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 1 bytes.");
            }
            return sbytes[0];
        }
        
    }
}
