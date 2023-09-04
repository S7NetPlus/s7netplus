using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Protocol;

namespace S7.Net.UnitTest.CommunicationTests;

[TestClass]
public class Clock
{
    [TestMethod, Timeout(1000)]
    public async Task Read_Clock_Value()
    {
        var cs = new CommunicationSequence
        {
            ConnectionOpenTemplates.ConnectionRequestConfirm,
            ConnectionOpenTemplates.CommunicationSetup,
            {
                """
                    // TPKT
                    03 00 00 1d

                    // COTP
                    02 f0 80

                    // S7 read clock
                    // UserData header
                    32 07 00 00 PDU1 PDU2
                    // Parameter length
                    00 08
                    // Data length
                    00 04

                    // Parameter
                    // Head
                    00 01 12
                    // Length
                    04
                    // Method (Request/Response): Req
                    11
                    // Type request (4...) Function group timers (...7)
                    47
                    // Subfunction: read clock
                    01
                    // Sequence number
                    00

                    // Data
                    // Return code
                    0a
                    // Transport size
                    00
                    // Payload length
                    00 00
                """,
                """
                    // TPKT
                    03 00 00 2b

                    // COTP
                    02 f0 80

                    // S7 read clock response
                    // UserData header
                    32 07 00 00 PDU1 PDU2
                    // Parameter length
                    00 0c
                    // Data length
                    00 0e

                    // Parameter
                    // Head
                    00 01 12
                    // Length
                    08
                    // Method (Request/Response): Res
                    12
                    // Type response (8...) Function group timers (...7)
                    87
                    // Subfunction: read clock
                    01
                    // Sequence number
                    01
                    // Data unit reference
                    00
                    // Last data unit? Yes
                    00
                    // Error code
                    00 00

                    // Data
                    // Error code
                    ff
                    // Transport size: OCTET STRING
                    09
                    // Length
                    00 0a

                    // Timestamp
                    // Reserved
                    00
                    // Year 1
                    19
                    // Year 2
                    14
                    // Month
                    08
                    // Day
                    20
                    // Hour
                    11
                    // Minute
                    59
                    // Seconds
                    43
                    // Milliseconds: 912..., Day of week: ...4
                    91 24
                """
            }
        };

        static async Task Client(int port)
        {
            var conn = new Plc(IPAddress.Loopback.ToString(), port, new TsapPair(new Tsap(1, 2), new Tsap(3, 4)));
            await conn.OpenAsync();
            var time = await conn.ReadClockAsync();

            Assert.AreEqual(new DateTime(2014, 8, 20, 11, 59, 43, 912), time);
            conn.Close();
        }

        await Task.WhenAll(cs.Serve(out var port), Client(port));
    }

    [TestMethod, Timeout(1000)]
    public async Task Write_Clock_Value()
    {
        var cs = new CommunicationSequence
        {
            ConnectionOpenTemplates.ConnectionRequestConfirm,
            ConnectionOpenTemplates.CommunicationSetup,
            {
                """
                    // TPKT
                    03 00 00 27

                    // COTP
                    02 f0 80

                    // S7 read clock
                    // UserData header
                    32 07 00 00 PDU1 PDU2
                    // Parameter length
                    00 08
                    // Data length
                    00 0e

                    // Parameter
                    // Head
                    00 01 12
                    // Length
                    04
                    // Method (Request/Response): Req
                    11
                    // Type request (4...) Function group timers (...7)
                    47
                    // Subfunction: write clock
                    02
                    // Sequence number
                    00

                    // Data
                    // Return code
                    ff
                    // Transport size
                    09
                    // Payload length
                    00 0a

                    // Payload
                    // Timestamp
                    // Reserved
                    00
                    // Year 1
                    19
                    // Year 2
                    14
                    // Month
                    08
                    // Day
                    20
                    // Hour
                    11
                    // Minute
                    59
                    // Seconds
                    43
                    // Milliseconds: 912..., Day of week: ...4
                    91 24
                """,
                """
                // TPKT
                    03 00 00 21

                    // COTP
                    02 f0 80

                    // S7 read clock response
                    // UserData header
                    32 07 00 00 PDU1 PDU2
                    // Parameter length
                    00 0c
                    // Data length
                    00 04

                    // Parameter
                    // Head
                    00 01 12
                    // Length
                    08
                    // Method (Request/Response): Res
                    12
                    // Type response (8...) Function group timers (...7)
                    87
                    // Subfunction: write clock
                    02
                    // Sequence number
                    01
                    // Data unit reference
                    00
                    // Last data unit? Yes
                    00
                    // Error code
                    00 00

                    // Data
                    // Error code
                    0a
                    // Transport size: NONE
                    00
                    // Length
                    00 00
                """
            }
        };

        static async Task Client(int port)
        {
            var conn = new Plc(IPAddress.Loopback.ToString(), port, new TsapPair(new Tsap(1, 2), new Tsap(3, 4)));
            await conn.OpenAsync();
            await conn.WriteClockAsync(new DateTime(2014, 08, 20, 11, 59, 43, 912));

            conn.Close();
        }

        await Task.WhenAll(cs.Serve(out var port), Client(port));
    }
}