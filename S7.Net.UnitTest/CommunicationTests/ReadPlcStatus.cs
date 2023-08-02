using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Protocol;

namespace S7.Net.UnitTest.CommunicationTests;

[TestClass]
public class ReadPlcStatus
{
    [TestMethod]
    public async Task Read_Status_Run()
    {
        var cs = new CommunicationSequence {
            ConnectionOpenTemplates.ConnectionRequestConfirm,
            ConnectionOpenTemplates.CommunicationSetup,
            {
                """
                    // TPKT
                    03 00 00 21

                    // COTP
                    02 f0 80

                    // S7 SZL read
                    32 07 00 00 PDU1 PDU2 00 08 00 08 00 01 12 04 11 44
                    01 00 ff 09 00 04 04 24 00 00
                """,
                """
                    // TPKT
                    03 00 00 3d

                    // COTP
                    02 f0 80

                    // S7 SZL response
                    32 07 00 00 PDU1 PDU2 00 0c 00 20 00 01 12 08 12 84
                    01 02 00 00 00 00 ff 09 00 1c 04 24 00 00 00 14
                    00 01 51 44 ff 08 00 00 00 00 00 00 00 00 14 08
                    20 12 05 28 34 94
                """
            }
        };

        async Task Client(int port)
        {
            var conn = new Plc(IPAddress.Loopback.ToString(), port, new TsapPair(new Tsap(1, 2), new Tsap(3, 4)));
            await conn.OpenAsync();
            var status = await conn.ReadStatusAsync();

            Assert.AreEqual(0x08, status);
            conn.Close();
        }

        await Task.WhenAll(cs.Serve(out var port), Client(port));
    }
}