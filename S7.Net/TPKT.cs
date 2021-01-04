using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace S7.Net
{

    /// <summary>
    /// Describes a TPKT Packet
    /// </summary>
    internal class TPKT
    {


        public byte Version;
        public byte Reserved1;
        public int Length;
        public byte[] Data;
        private TPKT(byte version, byte reserved1, int length, byte[] data)
        {
            Version = version;
            Reserved1 = reserved1;
            Length = length;
            Data = data;
        }

        /// <summary>
        /// Reads a TPKT from the socket
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>TPKT Instance</returns>
        public static TPKT Read(Stream stream)
        {
            var buf = new byte[4];
            int len = stream.ReadExact(buf, 0, 4);
            if (len < 4) throw new TPKTInvalidException($"TPKT header is incomplete / invalid. Received Bytes: {len} expected: {buf.Length}");
            var version = buf[0];
            var reserved1 = buf[1];
            var length = buf[2] * 256 + buf[3]; //BigEndian

            var data = new byte[length - 4];
            len = stream.ReadExact(data, 0, data.Length);
            if (len < data.Length)
                throw new TPKTInvalidException($"TPKT payload is incomplete / invalid. Received Bytes: {len} expected: {data.Length}");

            return new TPKT
            (
                version: version,
                reserved1: reserved1,
                length: length,
                data: data
            );
        }

        /// <summary>
        /// Reads a TPKT from the socket Async
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>Task TPKT Instace</returns>
        public static async Task<TPKT> ReadAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buf = new byte[4];
            int len = await stream.ReadExactAsync(buf, 0, 4, cancellationToken).ConfigureAwait(false);
            if (len < 4) throw new TPKTInvalidException("TPKT is incomplete / invalid");

            var version = buf[0];
            var reserved1 = buf[1];
            var length = buf[2] * 256 + buf[3]; //BigEndian

            var data = new byte[length - 4];
            len = await stream.ReadExactAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
            if (len < data.Length)
                throw new TPKTInvalidException("TPKT payload incomplete / invalid");

            return new TPKT
            (
                version: version,
                reserved1: reserved1,
                length: length,
                data: data
            );
        }

        public override string ToString()
        {
            return string.Format("Version: {0} Length: {1} Data: {2}",
                Version,
                Length,
                BitConverter.ToString(Data)
                );
        }
    }
}
