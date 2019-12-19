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

            Assert.AreEqual(DataType.Memory, dataItem.DataType, "Wrong datatype for M2000.1");
            Assert.AreEqual(0, dataItem.DB, "Wrong dbnumber for M2000.1");
            Assert.AreEqual(VarType.Bit, dataItem.VarType, "Wrong vartype for M2000.1");
            Assert.AreEqual(2000, dataItem.StartByteAdr, "Wrong startbyte for M2000.1");
            Assert.AreEqual(1, dataItem.BitAdr, "Wrong bit for M2000.1");
        }

        [TestMethod]
        public void T02_ParseMB200()
        {
            DataItem dataItem = DataItem.FromAddress("MB200");

            Assert.AreEqual(DataType.Memory, dataItem.DataType, "Wrong datatype for MB200");
            Assert.AreEqual(0, dataItem.DB, "Wrong dbnumber for MB200");
            Assert.AreEqual(VarType.Byte, dataItem.VarType, "Wrong vartype for MB200");
            Assert.AreEqual(200, dataItem.StartByteAdr, "Wrong startbyte for MB200");
            Assert.AreEqual(0, dataItem.BitAdr, "Wrong bit for MB200");
        }
    }
}
