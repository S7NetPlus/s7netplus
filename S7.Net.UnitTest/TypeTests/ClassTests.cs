using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class ClassTests
    {
        [TestMethod]
        public void GetClassSizeTest()
        {
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(1, 1)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 15)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 16)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 17)), 8);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 15)), 8);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 17)), 10);
        }

        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestUint32Read()
        {
            var result = new TestUint32();
            var data = new byte[4] { 0, 0, 0, 5 };
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(5u, result.Value1);
        }

        private class TestClassUnevenSize
        {
            public bool Bool { get; set; }
            public byte[] Bytes { get; set; }
            public bool[] Bools { get; set; }

            public TestClassUnevenSize(int byteCount, int bitCount)
            {
                Bytes = new byte[byteCount];
                Bools = new bool[bitCount];
            }
        }

        private class TestUint32
        {
            public uint Value1 { get; set; }
        }
    }
}
