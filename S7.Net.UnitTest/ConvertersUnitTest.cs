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
    }
}
