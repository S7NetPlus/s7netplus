using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boolean = S7.Net.Types.Boolean;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class BooleanTests
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        public void TestValidSetBitValues(int index)
        {
            Assert.AreEqual(Math.Pow(2, index), Boolean.SetBit(0, index));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        public void TestValidClearBitValues(int index)
        {
            Assert.AreEqual((byte) ((uint) Math.Pow(2, index) ^ uint.MaxValue), Boolean.ClearBit(byte.MaxValue, index));
        }
    }
}
