using System;
using System.Net.Sockets;

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
        /// Reds a TPKT from the socket
        /// </summary>
        /// <param name="socket">The socket to read from</param>
        /// <returns>TPKT Instace</returns>
        public static TPKT Read(Socket socket)
        {
            var buf = new byte[4];
            socket.Receive(buf, 4, SocketFlags.None);
            var pkt = new TPKT
            {
                Version = buf[0],
                Reserved1 = buf[1],
                Length = buf[2] * 256 + buf[3] //BigEndian
            };
            if (pkt.Length > 0)
            {
                pkt.Data = new byte[pkt.Length - 4];
                socket.Receive(pkt.Data, pkt.Length - 4, SocketFlags.None);
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
