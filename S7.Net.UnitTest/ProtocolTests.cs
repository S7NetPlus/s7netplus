using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Protocol;

namespace S7.Net.UnitTest
{
    [TestClass]
    public class ProtocolUnitTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TPKT_Read()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff0400807710000100000103000000033f8ccccd"));
            var t = await TPKT.ReadAsync(m, TestContext.CancellationTokenSource.Token);
            Assert.AreEqual(0x03, t.Version);
            Assert.AreEqual(0x29, t.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(TPKTInvalidException))]
        public async Task TPKT_ReadShort()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff040080"));
            var t = await TPKT.ReadAsync(m, CancellationToken.None);
        }


        [TestMethod]
        [ExpectedException(typeof(TPKTInvalidException))]
        public async Task TPKT_ReadShortAsync()
        {
            var m = new MemoryStream(StringToByteArray("0300002902f0803203000000010002001400000401ff040080"));
            var t = await TPKT.ReadAsync(m, TestContext.CancellationTokenSource.Token);
        }

        [TestMethod]
        public async Task COTP_ReadTSDU()
        {
            var expected = StringToByteArray("320700000400000800080001120411440100ff09000400000000");
            var m = new MemoryStream(StringToByteArray("0300000702f0000300000702f0000300002102f080320700000400000800080001120411440100ff09000400000000"));
            var t = await COTP.TSDU.ReadAsync(m, TestContext.CancellationTokenSource.Token);
            Assert.IsTrue(expected.SequenceEqual(t));
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        [TestMethod]
        public async Task TestResponseCode()
        {
            var expected = StringToByteArray("320700000400000800080001120411440100ff09000400000000");
            var m = new MemoryStream(StringToByteArray("0300000702f0000300000702f0000300002102f080320700000400000800080001120411440100ff09000400000000"));
            var t = await COTP.TSDU.ReadAsync(m, CancellationToken.None);
            Assert.IsTrue(expected.SequenceEqual(t));

            // Test all possible byte values. Everything except 0xff should throw an exception.
            var testData = Enumerable.Range(0, 256).Select(i => new { StatusCode = (ReadWriteErrorCode)i, ThrowsException = i != (byte)ReadWriteErrorCode.Success });

            foreach (var entry in testData)
            {
                if (entry.ThrowsException)
                {
                    Assert.ThrowsException<Exception>(() => Plc.ValidateResponseCode(entry.StatusCode));
                }
                else
                {
                    Plc.ValidateResponseCode(entry.StatusCode);
                }
            }
        }
    }

}
