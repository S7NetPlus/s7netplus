using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class StringTests
    {
        [TestMethod]
        public void WriteNullWIthReservedLengthZero()
        {
            AssertToByteArrayEquals(null, 0);
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthZero()
        {
            AssertToByteArrayEquals("", 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthZero()
        {
            AssertToByteArrayEquals("A", 0);
        }

        [TestMethod]
        public void WriteNullWithReservedLengthOne()
        {
            AssertToByteArrayEquals(null, 1, 0);
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthOne()
        {
            AssertToByteArrayEquals("", 1, 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthOne()
        {
            AssertToByteArrayEquals("A", 1, (byte) 'A');
        }

        [TestMethod]
        public void WriteAWithReservedLengthTwo()
        {
            AssertToByteArrayEquals("A", 2, (byte) 'A', 0);
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthOne()
        {
            AssertToByteArrayEquals("Abc", 1, (byte) 'A');
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthTwo()
        {
            AssertToByteArrayEquals("Abc", 2, (byte) 'A', (byte) 'b');
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthThree()
        {
            AssertToByteArrayEquals("Abc", 3, (byte) 'A', (byte) 'b', (byte) 'c');
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthFour()
        {
            AssertToByteArrayEquals("Abc", 4, (byte) 'A', (byte) 'b', (byte) 'c', 0);
        }

        private static void AssertFromByteArrayEquals(string expected, params byte[] bytes)
        {
            Assert.AreEqual(expected, String.FromByteArray(bytes));
        }

        private static void AssertToByteArrayEquals(string value, int reservedLength, params byte[] expected)
        {
            CollectionAssert.AreEqual(expected, String.ToByteArray(value, reservedLength));
        }
    }
}
