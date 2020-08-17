using System;
using System.IO;
using System.Net.Sockets;
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
        /// Reads a fixed amount of bytes from the stream into the buffer
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <returns>returns the amount of read bytes</returns>
        public static int ReadExact(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream is NetworkStream network)
            {
                return network.ReadExact(buffer, offset, count, network.ReadTimeout > 0 ? network.ReadTimeout : 500);
            }
            else if ((stream.CanTimeout && stream.ReadTimeout > 0) || stream.CanSeek)
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
            else
            {
                throw new NotSupportedException("Reading fixed buffer sizes from a stream is only supported for networkstreams or streams with ReadTimeout greater than 0.");
            }
        }


        /// <summary>
        /// Reads a fixed amount of bytes from the stream into the buffer
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <param name="timeoutMs">The timeout to abort the read</param>
        /// <returns>returns the amount of read bytes</returns>
        /// <exception cref="TimeoutException">
        ///  Throws timeout exception, when the timeout elapsed, while waiting for available Data
        /// </exception>
        public static int ReadExact(this NetworkStream stream, byte[] buffer, int offset, int count, int timeoutMs)
        {
            int read = offset;
            int received = 0;
            count = Math.Min(count, buffer.Length - offset);
            do
            {
                if (stream.DataAvailable)
                {
                    received = stream.Read(buffer, read, count - read);
                    read += received;
                }
                else
                {
                    var timedOut = SpinWait.SpinUntil(() => stream.DataAvailable, 10);
                    timeoutMs -= 10;
                    received = 1;
                }
            }
            while (read < count && !(timeoutMs <= 0 || received <= 0));

            if (read < count && timeoutMs <= 0)
            {
                throw new TimeoutException($"Timeout receiving desired size. Missing {count - read} bytes");
            }

            return read;
        }

        /// <summary>
        /// Reads a fixed amount of bytes from the stream into the buffer
        /// </summary>
        /// <param name="stream">the Stream to read from</param>
        /// <param name="buffer">the buffer to read into</param>
        /// <param name="offset">the offset in the buffer to read into</param>
        /// <param name="count">the amount of bytes to read into the buffer</param>
        /// <param name="token">the token to abort the operation</param>
        /// <returns>returns the amount of read bytes</returns>
        public static async Task<int> ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken token)
        {
            int read = offset;
            int received;
            count = Math.Min(count, buffer.Length - offset);
            do
            {
                received = await stream.ReadAsync(buffer, read, count - read, token);
                read += received;
            }
            while (read < count && received > 0);

            return read;
        }
    }
}