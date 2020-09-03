using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace S7.Net.UnitTest
{
    /// <summary>
    /// Test stream which only gives 1 byte per read.
    /// </summary>
    class TestStream1BytePerRead : Stream
    {
        public TestStream1BytePerRead(byte[] data)
        {
            Data = data;
        }
        public override bool CanRead => _position < Data.Length;

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[] Data { get; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        int _position = 0;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= Data.Length)
            {
                return 0;
            }
            buffer[offset] = Data[_position];
            ++_position;
            return 1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// These tests are intended to test <see cref="StreamExtensions"/> functions and other stream-related special cases.
    /// </summary>
    [TestClass]
    public class StreamTests
    {

        [TestMethod]
        public async Task TPKT_ReadRestrictedStreamAsync()
        {
            var fullMessage = ProtocolUnitTest.StringToByteArray("0300002902f0803203000000010002001400000401ff0400807710000100000103000000033f8ccccd");
            var m = new TestStream1BytePerRead(fullMessage);
            var t = await TPKT.ReadAsync(m);
            Assert.AreEqual(fullMessage.Length, t.Length);
            Assert.AreEqual(fullMessage.Last(), t.Data.Last());
        }

        [TestMethod]
        public void TPKT_ReadRestrictedStream()
        {
            var fullMessage = ProtocolUnitTest.StringToByteArray("0300002902f0803203000000010002001400000401ff0400807710000100000103000000033f8ccccd");
            var m = new TestStream1BytePerRead(fullMessage);
            var t = TPKT.Read(m);
            Assert.AreEqual(fullMessage.Length, t.Length);
            Assert.AreEqual(fullMessage.Last(), t.Data.Last());
        }

        [TestMethod]
        public void TPKT_ReadStreamTooShort()
        {
            var fullMessage = ProtocolUnitTest.StringToByteArray("0300002902f0803203000000010002001400");
            var m = new TestStream1BytePerRead(fullMessage);
            Assert.ThrowsException<TPKTInvalidException>(() => TPKT.Read(m));
        }
    }
}
