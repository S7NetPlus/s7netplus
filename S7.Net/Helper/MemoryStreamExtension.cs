using System;
using System.Buffers;
using System.IO;

namespace S7.Net.Helper
{
#if !NET5_0_OR_GREATER
    internal static class MemoryStreamExtension
    {
        /// <summary>
        /// Helper function to write to whole content of the given byte array to a memory stream.
        /// 
        /// Writes all bytes in value from 0 to value.Length to the memory stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void Write(this MemoryStream stream, byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }

        /// <summary>
        /// Helper function to write the whole content of the given byte span to a memory stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void Write(this MemoryStream stream, ReadOnlySpan<byte> value)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(value.Length);

            value.CopyTo(buffer);
            stream.Write(buffer, 0, value.Length);

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
#endif
}
