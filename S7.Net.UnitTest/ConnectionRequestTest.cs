using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Protocol;

namespace S7.Net.UnitTest
{
    [TestClass]
    public class ConnectionRequestTest
    {
        [TestMethod]
        public void Test_ConnectionRequest_S7_200()
        {
            CollectionAssert.AreEqual(MakeConnectionRequest(16, 0, 16, 1),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7200, 0, 0)));
        }

        [TestMethod]
        public void Test_ConnectionRequest_S7_300()
        {
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 0),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7300, 0, 0)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 1),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7300, 0, 1)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 33),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7300, 1, 1)));
        }

        [TestMethod]
        public void Test_ConnectionRequest_S7_400()
        {
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 0),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7400, 0, 0)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 1),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7400, 0, 1)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 33),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S7400, 1, 1)));
        }

        [TestMethod]
        public void Test_ConnectionRequest_S7_1200()
        {
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 0),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71200, 0, 0)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 1),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71200, 0, 1)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 33),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71200, 1, 1)));
        }

        [TestMethod]
        public void Test_ConnectionRequest_S7_1500()
        {
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 0),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71500, 0, 0)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 1),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71500, 0, 1)));
            CollectionAssert.AreEqual(MakeConnectionRequest(1, 0, 3, 33),
                ConnectionRequest.GetCOTPConnectionRequest(TsapPair.GetDefaultTsapPair(CpuType.S71500, 1, 1)));
        }

        private static byte[] MakeConnectionRequest(byte sourceTsap1, byte sourceTsap2, byte destTsap1, byte destTsap2)
        {
            return new byte[]
            {
                3, 0, 0, 22, //TPKT
                17, //COTP Header Length
                224, //Connect Request
                0, 0, //Destination Reference
                0, 46, //Source Reference
                0, //Flags
                193, //Parameter Code (src-tasp)
                2, //Parameter Length
                sourceTsap1, sourceTsap2, //Source TASP
                194, //Parameter Code (dst-tasp)
                2, //Parameter Length
                destTsap1, destTsap2, //Destination TASP
                192, //Parameter Code (tpdu-size)
                1, //Parameter Length
                10 //TPDU Size (2^11 = 2048)
            };
        }
    }
}
