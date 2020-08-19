using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;
using System;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class StringExTests
    {
        [TestMethod]
        public void ReadEmptyStringWithZeroByteLength()
        {
            AssertFromByteArrayEquals("", 0, 0);
        }

        [TestMethod]
        public void ReadEmptyStringWithOneByteLength()
        {
            AssertFromByteArrayEquals("", 1, 0, 0);
        }

        [TestMethod]
        public void ReadEmptyStringWithOneByteGarbage()
        {
            AssertFromByteArrayEquals("", 1, 0, (byte) 'a');
        }

        [TestMethod]
        public void ReadA()
        {
            AssertFromByteArrayEquals("A", 1, 1, (byte) 'A');
        }

        [TestMethod]
        public void ReadAbc()
        {
            AssertFromByteArrayEquals("Abc", 1, 3, (byte) 'A', (byte) 'b', (byte) 'c');
        }

        [TestMethod]
        public void WriteNullWithReservedLengthZero()
        {
            Assert.ThrowsException<ArgumentNullException>(() => AssertToByteArrayEquals(null, 0, 0, 0));
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthZero()
        {
            AssertToByteArrayEquals("", 0, 0, 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthZero()
        {
            AssertToByteArrayEquals("", 0, 0, 0);
        }

        [TestMethod]
        public void WriteNullWithReservedLengthOne()
        {
            Assert.ThrowsException<ArgumentNullException>(() => AssertToByteArrayEquals(null, 1, 1, 0));
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthOne()
        {
            AssertToByteArrayEquals("", 1, 1, 0, 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthOne()
        {
            AssertToByteArrayEquals("A", 1, 1, 1, (byte) 'A');
        }

        [TestMethod]
        public void WriteAWithReservedLengthTwo()
        {
            AssertToByteArrayEquals("A", 2, 2, 1, (byte) 'A', 0);
        }

        [TestMethod]
        public void WriteAbcWithStringLargetThanReservedLength()
        {
            Assert.ThrowsException<ArgumentException>(() => StringEx.ToByteArray("Abc", 2));
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthThree()
        {
            AssertToByteArrayEquals("Abc", 3, 3, 3, (byte) 'A', (byte) 'b', (byte) 'c');
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthFour()
        {
            AssertToByteArrayEquals("Abc", 4, 4, 3, (byte) 'A', (byte) 'b', (byte) 'c', 0);
        }

        private static void AssertFromByteArrayEquals(string expected, params byte[] bytes)
        {
            Assert.AreEqual(expected, StringEx.FromByteArray(bytes));
        }

        private static void AssertToByteArrayEquals(string value, int reservedLength, params byte[] expected)
        {
            var convertedData = StringEx.ToByteArray(value, reservedLength);
            CollectionAssert.AreEqual(expected, convertedData);
        }
    }
}
