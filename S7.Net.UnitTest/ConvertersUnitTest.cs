using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net;

namespace S7.Net.UnitTest
{
    [TestClass]
    public class ConvertersUnitTest
    {
        [TestMethod]
        public void T00_TestSelectBit()
        {
            byte dummyByte = 5; // 0000 0101
            Assert.IsTrue(dummyByte.SelectBit(0));
            Assert.IsFalse(dummyByte.SelectBit(1));
            Assert.IsTrue(dummyByte.SelectBit(2));
            Assert.IsFalse(dummyByte.SelectBit(3));
            Assert.IsFalse(dummyByte.SelectBit(4));
            Assert.IsFalse(dummyByte.SelectBit(5));
            Assert.IsFalse(dummyByte.SelectBit(6));
            Assert.IsFalse(dummyByte.SelectBit(7));

        }

        [TestMethod]
        public void T01_TestSetBit()
        {
            byte dummyByte = 0xAA; // 1010 1010
            dummyByte.SetBit(0, true);
            dummyByte.SetBit(1, false);
            dummyByte.SetBit(2, true);
            dummyByte.SetBit(3, false);
            Assert.AreEqual<byte>(dummyByte, 0xA5);// 1010 0101
            dummyByte.SetBit(4, true);
            dummyByte.SetBit(5, true);
            dummyByte.SetBit(6, true);
            dummyByte.SetBit(7, true);
            Assert.AreEqual<byte>(dummyByte, 0xF5);// 1111 0101
        }
    }
}
