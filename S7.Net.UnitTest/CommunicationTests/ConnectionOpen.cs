using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Protocol;

namespace S7.Net.UnitTest.CommunicationTests;

[TestClass]
public class ConnectionOpen
{
    [TestMethod]
    public async Task Does_Not_Throw()
    {
        var cs = new CommunicationSequence {
            ConnectionOpenTemplates.ConnectionRequestConfirm,
            ConnectionOpenTemplates.CommunicationSetup
        };

        async Task Client(int port)
        {
            var conn = new Plc(IPAddress.Loopback.ToString(), port, new TsapPair(new Tsap(1, 2), new Tsap(3, 4)));
            await conn.OpenAsync();
            conn.Close();
        }

        await Task.WhenAll(cs.Serve(out var port), Client(port));
    }
}