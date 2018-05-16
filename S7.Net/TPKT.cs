using System;
using System.Net.Sockets;
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

        private static byte[] sharedBuffer = new byte[4096];

        /// <summary>
        /// Reads a TPKT from the socket
        /// </summary>
        /// <param name="socket">The stream to read from</param>
        /// <returns>TPKT Instance</returns>
        public static TPKT Read(Socket socket)
        {
            var buf = new byte[4];
            int len = socket.Receive(buf, 0, 4, SocketFlags.None);
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
                len = socket.Receive(pkt.Data, 0, pkt.Length - 4, SocketFlags.None);
                if (len < pkt.Length - 4)
                    throw new TPKTInvalidException("TPKT is incomplete / invalid");
            }
            return pkt;
        }

        /// <summary>
        /// Reads a TPKT from the socket Async
        /// </summary>
        /// <param name="socket">The stream to read from</param>
        /// <returns>Task TPKT Instace</returns>
        public static async Task<TPKT> ReadAsync(Socket socket)
        {
            int len = await socket.ReadAsync(sharedBuffer, 0, 4);
            if (len < 4) throw new TPKTInvalidException("TPKT is incomplete / invalid");
            var pkt = new TPKT
            {
                Version = sharedBuffer[0],
                Reserved1 = sharedBuffer[1],
                Length = sharedBuffer[2] * 256 + sharedBuffer[3] //BigEndian
            };
            if (pkt.Length > 0)
            {
                pkt.Data = new byte[pkt.Length - 4];
                len = await socket.ReadAsync(pkt.Data, 0, pkt.Length - 4);
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
