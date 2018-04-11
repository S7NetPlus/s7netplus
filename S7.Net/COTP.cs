using System;
using System.IO;
using System.Net.Sockets;

namespace S7.Net
{

    /// <summary>
    /// COTP Protocol functions and types
    /// </summary>
    internal class COTP
    {
        /// <summary>
        /// Describes a COTP TPDU (Transport protocol data unit)
        /// </summary>
        public class TPDU
        {
            public byte HeaderLength;
            public byte PDUType;
            public int TPDUNumber;
            public byte[] Data;
            public bool LastDataUnit;

            public TPDU(TPKT tPKT)
            {
                var br = new BinaryReader(new MemoryStream(tPKT.Data));
                HeaderLength = br.ReadByte();
                if (HeaderLength >= 2)
                {
                    PDUType = br.ReadByte();
                    if (PDUType == 0xf0) //DT Data
                    {
                        var flags = br.ReadByte();
                        TPDUNumber = flags & 0x7F;
                        LastDataUnit = (flags & 0x80) > 0;
                        Data = br.ReadBytes(tPKT.Length - HeaderLength - 4); //4 = TPKT Size
                        return;
                    }
                    //TODO: Handle other PDUTypes
                }
                Data = new byte[0];
            }

            /// <summary>
            /// Reads COTP TPDU (Transport protocol data unit) from the network stream
            /// See: https://tools.ietf.org/html/rfc905
            /// </summary>
            /// <param name="socket">The socket to read from</param>
            /// <returns>COTP DPDU instance</returns>
            public static TPDU Read(Socket socket)
            {
                var tpkt = TPKT.Read(socket);
                return new TPDU(tpkt);
            }

            public override string ToString()
            {
                return string.Format("Length: {0} PDUType: {1} TPDUNumber: {2} Last: {3} Segment Data: {4}",
                    HeaderLength,
                    PDUType,
                    TPDUNumber,
                    LastDataUnit,
                    BitConverter.ToString(Data)
                    );
            }

        }

        /// <summary>
        /// Describes a COTP TSDU (Transport service data unit). One TSDU consist of 1 ore more TPDUs
        /// </summary>
        public class TSDU
        {
            /// <summary>
            /// Reads the full COTP TSDU (Transport service data unit)
            /// See: https://tools.ietf.org/html/rfc905
            /// </summary>
            /// <returns>Data in TSDU</returns>
            public static byte[] Read(Socket socket)
            {                
                var segment = TPDU.Read(socket);
                var output = new MemoryStream(segment.Data.Length);
                output.Write(segment.Data, 0, segment.Data.Length);

                while (!segment.LastDataUnit)
                {
                    segment = TPDU.Read(socket);
                    output.Write(segment.Data, (int)output.Position, segment.Data.Length);
                }
                return output.GetBuffer();
            }
        }
    }
}
