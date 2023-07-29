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
        /// Reads a TPKT from the socket Async
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
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
