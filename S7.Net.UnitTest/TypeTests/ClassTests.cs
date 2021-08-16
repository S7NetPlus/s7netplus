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
    }
}
