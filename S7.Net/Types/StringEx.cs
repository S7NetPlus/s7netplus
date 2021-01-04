using System;

namespace S7.Net.Types
{
    /// <inheritdoc cref="S7String"/>
    [Obsolete("Please use S7String class")]
    public static class StringEx
    {
        /// <inheritdoc cref="S7String.FromByteArray(byte[])"/>
        public static string FromByteArray(byte[] bytes) => S7String.FromByteArray(bytes);

        /// <inheritdoc cref="S7String.ToByteArray(string, int)"/>
        public static byte[] ToByteArray(string value, int reservedLength) => S7String.ToByteArray(value, reservedLength);
    }
}
