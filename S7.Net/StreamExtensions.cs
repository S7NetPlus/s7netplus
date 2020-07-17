using System;
using System.IO;
using System.Threading.Tasks;

namespace S7.Net
{
    /// <summary>
    /// Extensions for Streams
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads a fixed amount of bytes from the stream into the buffer
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <returns>returns the amount of read bytes</returns>
        public static int ReadFixed(this Stream stream, byte[] buffer, int offset, int count)
        {
            int read = offset;
            int received;
            count = Math.Min(count, buffer.Length - offset);
            do
            {
                received = stream.Read(buffer, read, count - read);
                read += received;
            }
            while (read < count && received > 0);

            return read;
        }

        /// <summary>
        /// Reads a fixed amount of bytes from the stream into the buffer
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <returns>returns the amount of read bytes</returns>
        public static async Task<int> ReadFixedAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            int read = offset;
            int received;
            count = Math.Min(count, buffer.Length - offset);
            do
            {
                received = await stream.ReadAsync(buffer, read, count - read);
                read += received;
            }
            while (read < count && received > 0);

            return read;
        }
    }
}