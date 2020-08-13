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
