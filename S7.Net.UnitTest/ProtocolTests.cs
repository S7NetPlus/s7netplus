using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net;

using System.IO;
using System.Threading.Tasks;

namespace S7.Net.UnitTest
{
    [TestClass]
    public class ProtocolUnitTest
    {
        [TestMethod]
        public void TPKT_Read()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff0400807710000100000103000000033f8ccccd"));
            var t = TPKT.Read(m);
            Assert.AreEqual(0x03, t.Version);
            Assert.AreEqual(0x29, t.Length);
            m.Position = 0;
            t = TPKT.ReadAsync(m).Result;
            Assert.AreEqual(0x03, t.Version);
            Assert.AreEqual(0x29, t.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(TPKTInvalidException))]
        public void TPKT_ReadShort()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff040080"));
            var t = TPKT.Read(m);
        }

        [TestMethod]
        [ExpectedException(typeof(TPKTInvalidException))]
        public async Task TPKT_ReadShortAsync()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff040080"));
            var t = await TPKT.ReadAsync(m);
         }

        [TestMethod]
        public void COTP_ReadTSDU()
        {
            var expected = StringToByteArray("320700000400000800080001120411440100ff09000400000000");
            var m = new MemoryStream(StringToByteArray("0300000702f0000300000702f0000300002102f080320700000400000800080001120411440100ff09000400000000"));
            var t = COTP.TSDU.Read(m);
            Assert.IsTrue(expected.SequenceEqual(t));
            m.Position = 0;
            t = COTP.TSDU.ReadAsync(m).Result;
            Assert.IsTrue(expected.SequenceEqual(t));
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
