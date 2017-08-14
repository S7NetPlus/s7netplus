using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Contains the methods to read, set and reset bits inside bytes
    /// </summary>
    public static class Boolean
    {
        /// <summary>
        /// Converts a bit to byte array
        /// </summary>
        public static byte[] ToByteArray(bool value)
        {
            byte[] bytes = new byte[] { value ? (byte)1 : (byte)0 };
            return bytes;
        }
    }
}
