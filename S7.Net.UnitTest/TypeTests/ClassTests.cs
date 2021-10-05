using System;
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
                CpuType type;
                foreach (int value in Enum.GetValues(typeof(CpuType)))
                {
                    type = (CpuType)value;
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(1, 1), cpu: type), 6);
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 15), cpu: type), 6);
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 16), cpu: type), 6);
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 17), cpu: type), 8);
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 15), cpu: type), 8);
                    Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 17), cpu: type), 10);
                }
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
        
        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestInt32Read()
        {
            var result = new TestInt32();
            var data = new byte[4] { 1, 0, 0, 128 };
            int expected = 16777344;
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(expected, result.Value1);
        }
        
        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestInt32ReadNegative()
        {
            var result = new TestInt32();
            var data = new byte[4] { 128, 8, 4, 1 };
            int expected = -2146958335;
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(expected, result.Value1);
        }
        
        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestUint64Read()
        {
            var result = new TestUint64();
            var data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 5 };
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(5ul, result.Value1);
        }
        
        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestInt64Read()
        {
            var result = new TestInt64();
            var data = new byte[8] { 1, 0, 0, 0, 1, 0, 0, 128 };
            long expected = 72057594054705280;
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(expected, result.Value1);
        }
        
        /// <summary>
        /// Ensure Uint32 is correctly parsed through ReadClass functions. Adresses issue https://github.com/S7NetPlus/s7netplus/issues/414
        /// </summary>
        [TestMethod]
        public void TestInt64ReadNegative()
        {
            var result = new TestInt64();
            var data = new byte[8] { 128, 0, 0, 0, 0, 8, 4, 1 };
            long expected = -9223372036854250495;
            var bytesRead = Class.FromBytes(result, data);
            Assert.AreEqual(bytesRead, data.Length);
            Assert.AreEqual(expected, result.Value1);
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
        
        private class TestInt32
        {
            public int Value1 { get; set; }
        }
        
        private class TestUint64
        {
            public ulong Value1 { get; set; }
        }
        
        private class TestInt64
        {
            public long Value1 { get; set; }
        }
    }
}
