using System;
using System.IO;
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

        /// <summary>
        /// Reads a TPKT from the socket
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>TPKT Instance</returns>
        public static TPKT Read(Stream stream)
        {
            var buf = new byte[4];
            int len = stream.Read(buf, 0, 4);
            if (len < 4) throw new TPKTInvalidException("TPKT is incomplete / invalid");
            var pkt = new TPKT
            {
                Version = buf[0],
                Reserved1 = buf[1],
                Length = buf[2] * 256 + buf[3] //BigEndian
            };
            if (pkt.Length > 0)
            {
                pkt.Data = new byte[pkt.Length - 4];
                len = stream.Read(pkt.Data, 0, pkt.Length - 4);
                if (len < pkt.Length - 4)
                    throw new TPKTInvalidException("TPKT is incomplete / invalid");
            }
            return pkt;
        }

        /// <summary>
        /// Reads a TPKT from the socket Async
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>Task TPKT Instace</returns>
        public static async Task<TPKT> ReadAsync(Stream stream)
        {
            var buf = new byte[4];
            int len = await stream.ReadAsync(buf, 0, 4);
            if (len < 4) throw new TPKTInvalidException("TPKT is incomplete / invalid");
            var pkt = new TPKT
            {
                Version = buf[0],
                Reserved1 = buf[1],
                Length = buf[2] * 256 + buf[3] //BigEndian
            };
            if (pkt.Length > 0)
            {
                pkt.Data = new byte[pkt.Length - 4];
                len = await stream.ReadAsync(pkt.Data, 0, pkt.Length - 4);
                if (len < pkt.Length - 4) throw new TPKTInvalidException("TPKT is incomplete / invalid");
            }
            return pkt;
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
