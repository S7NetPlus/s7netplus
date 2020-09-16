using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace S7.Net
{
    /// <summary>
    /// Extensions for Streams
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads bytes from the stream into the buffer until exactly the requested number of bytes (or EOF) have been read
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <returns>returns the amount of read bytes</returns>
        public static int ReadExact(this Stream stream, byte[] buffer, int offset, int count)
        {
            int read = 0;
            int received;
            do
            {
                received = stream.Read(buffer, offset + read, count - read);
                read += received;
            }
            while (read < count && received > 0);

            return read;
        }

        /// <summary>
        /// Reads bytes from the stream into the buffer until exactly the requested number of bytes (or EOF) have been read
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <returns>returns the amount of read bytes</returns>
        public static async Task<int> ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = 0;
            int received;
            do
            {
                received = await stream.ReadAsync(buffer, offset + read, count - read, cancellationToken).ConfigureAwait(false);
                read += received;
            }
            while (read < count && received > 0);

            return read;
        }
    }
}