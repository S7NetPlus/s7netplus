using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;
using System;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class S7WStringTests
    {
        [TestMethod]
        public void ReadEmptyStringWithZeroLength()
        {
            AssertFromByteArrayEquals("", 0, 0 , 0, 0);
        }

        [TestMethod]
        public void ReadEmptyStringWithOneCharLength()
        {
            AssertFromByteArrayEquals("", 0, 1, 0, 0, 0, 0);
        }

        [TestMethod]
        public void ReadEmptyStringWithOneCharGarbage()
        {

            AssertFromByteArrayEquals("", 0, 1, 0, 0, 0x00, 0x41);
        }

        [TestMethod]
        public void ReadMalformedStringTooShort()
        {
            Assert.ThrowsException<PlcException>(() => AssertFromByteArrayEquals("", 0, 1));
        }

        [TestMethod]
        public void ReadMalformedStringSizeLargerThanCapacity()
        {
            Assert.ThrowsException<PlcException>(() => S7WString.FromByteArray(new byte[] { 0, 3, 0, 5, 0, 0x00, 0x41, 0x00, 0x41, 0x00, 0x41}));
        }

        [TestMethod]
        public void ReadMalformedStringCapacityTooLarge()
        {
            Assert.ThrowsException<ArgumentException>(() => AssertToByteArrayAndBackEquals("", 20000, 0));
        }

        [TestMethod]
        public void ReadA()
        {
            AssertFromByteArrayEquals("A", 0, 1, 0, 1, 0x00, 0x41);
        }

        [TestMethod]
        public void ReadAbc()
        {
            AssertFromByteArrayEquals("Abc", 0, 3, 0, 3, 0x00, 0x41, 0x00, 0x62, 0x00, 0x63);
        }

        [TestMethod]
        public void WriteNullWithReservedLengthZero()
        {
            Assert.ThrowsException<ArgumentNullException>(() => AssertToByteArrayAndBackEquals(null, 0, 0, 0, 0, 0));
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthZero()
        {
            AssertToByteArrayAndBackEquals("", 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthZero()
        {
            AssertToByteArrayAndBackEquals("", 0, 0, 0, 0, 0);
        }

        [TestMethod]
        public void WriteNullWithReservedLengthOne()
        {
            Assert.ThrowsException<ArgumentNullException>(() => AssertToByteArrayAndBackEquals(null, 1, 0, 1 , 0, 0));
        }

        [TestMethod]
        public void WriteEmptyStringWithReservedLengthOne()
        {
            AssertToByteArrayAndBackEquals("", 1, 0, 1, 0, 0, 0, 0);
        }

        [TestMethod]
        public void WriteAWithReservedLengthOne()
        {
            AssertToByteArrayAndBackEquals("A", 1, 0, 1, 0, 1, 0x00, 0x41);
        }

        [TestMethod]
        public void WriteAWithReservedLengthTwo()
        {
            AssertToByteArrayAndBackEquals("A", 2, 0, 2, 0, 1, 0x00, 0x41, 0, 0);
        }

        [TestMethod]
        public void WriteAbcWithStringLargerThanReservedLength()
        {
            Assert.ThrowsException<ArgumentException>(() => S7WString.ToByteArray("Abc", 2));
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthThree()
        {
            AssertToByteArrayAndBackEquals("Abc", 3, 0, 3, 0, 3, 0x00, 0x41, 0x00, 0x62, 0x00, 0x63);
        }

        [TestMethod]
        public void WriteAbcWithReservedLengthFour()
        {
            AssertToByteArrayAndBackEquals("Abc", 4, 0, 4, 0, 3, 0x00, 0x41, 0x00, 0x62, 0x00, 0x63, 0 , 0);
        }

        private static void AssertFromByteArrayEquals(string expected, params byte[] bytes)
        {
            var convertedString = S7WString.FromByteArray(bytes);
            Assert.AreEqual(expected, convertedString);
        }

        [TestMethod]
        public void OddS7WStringByteLength()
        {
            AssertVarTypeToByteLength(VarType.S7WString, 1, 6);
        }

        [TestMethod]
        public void EvenS7WStringByteLength()
        {
            AssertVarTypeToByteLength(VarType.S7WString, 2, 8);
        }

        private static void AssertToByteArrayAndBackEquals(string value, int reservedLength, params byte[] expected)
        {
            var convertedData = S7WString.ToByteArray(value, reservedLength);
            CollectionAssert.AreEqual(expected, convertedData);
            var convertedBack = S7WString.FromByteArray(convertedData);
            Assert.AreEqual(value, convertedBack);
        }

        private void AssertVarTypeToByteLength(VarType varType, int count, int expectedByteLength)
        {
            var byteLength = Plc.VarTypeToByteLength(varType, count);
            Assert.AreEqual(expectedByteLength, byteLength);
        }
    }
}
