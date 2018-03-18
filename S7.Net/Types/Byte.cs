using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to convert from bytes to byte arrays
    /// </summary>
    public static class Byte
    {
        /// <summary>
        /// Converts a byte to byte array
        /// </summary>
        public static byte[] ToByteArray(byte value)
        {
            return new byte[] { value }; ;
        }
       
        /// <summary>
        /// Converts a byte array to byte
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte FromByteArray(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                throw new ArgumentException("Wrong number of bytes. Bytes array must contain 1 bytes.");
            }
            return bytes[0];
        }
        
    }
}
