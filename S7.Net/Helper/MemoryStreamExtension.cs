
namespace S7.Net.Helper
{
    internal static class MemoryStreamExtension
    {
        /// <summary>
        /// Helper function to write to whole content of the given byte array to a memory stream.
        /// 
        /// Writes all bytes in value from 0 to value.Length to the memory stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteByteArray(this System.IO.MemoryStream stream, byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }
    }
}
