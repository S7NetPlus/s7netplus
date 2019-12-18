using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;
using System;

namespace S7.Net.UnitTest
{
    [TestClass]
    public class PLCAddressParsingTests
    {
        [TestMethod]
        public void T01_ParseM2000_1()
        {
            DataItem dataItem = DataItem.FromAddress("M2000.1");

            Assert.AreEqual(dataItem.DataType, DataType.Memory, "Wrong datatype for M2000.1");
            Assert.AreEqual(dataItem.DB, 0, "Wrong dbnumber for M2000.1");
            Assert.AreEqual(dataItem.VarType, VarType.Bit, "Wrong vartype for M2000.1");
            Assert.AreEqual(dataItem.StartByteAdr, 2000, "Wrong startbyte for M2000.1");
            Assert.AreEqual(dataItem.BitAdr, 1, "Wrong bit for M2000.1");


        }

    }
}
