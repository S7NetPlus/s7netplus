using System;
using System.IO;
using System.Runtime.InteropServices;

namespace S7.Net.Types
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Converts an array of T to an array of bytes 
        /// </summary>
        public static byte[] ToByteArray<T>(T[] value, Func<T, byte[]> converter) where T : struct
        {
            var buffer = new byte[Marshal.SizeOf(default(T)) * value.Length];
            var stream = new MemoryStream(buffer);
            foreach (var val in value)
            {
                stream.Write(converter(val), 0, 4);
            }

            return buffer;
        }

        /// <summary>
        /// Converts an array of T repesented as S7 binary data to an array of T
        /// </summary>
        public static T[] ToArray<T>(byte[] bytes, Func<byte[], T> converter) where T : struct
        {
            var typeSize = Marshal.SizeOf(default(T));
            var entries = bytes.Length / typeSize;
            var values = new T[entries];

            for(int i = 0; i < entries; ++i)
            {
                var buffer = new byte[typeSize];
                Array.Copy(bytes, i * typeSize, buffer, 0, typeSize);
                values[i] = converter(buffer);
            }

            return values;
        }
    }
}
