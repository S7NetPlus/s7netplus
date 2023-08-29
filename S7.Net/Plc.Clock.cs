using System;
using System.IO;
using System.Linq;
using S7.Net.Helper;
using S7.Net.Types;
using DateTime = System.DateTime;

namespace S7.Net;

partial class Plc
{
    private const byte SzlFunctionGroupTimers = 0x07;
    private const byte SzlSubFunctionReadClock = 0x01;

    private static byte[] BuildClockReadRequest()
    {
        var stream = new MemoryStream();

        WriteSzlRequestHeader(stream, SzlFunctionGroupTimers, SzlSubFunctionReadClock, 4);
        stream.Write(new byte[] { 0x0a, 0x00, 0x00, 0x00 });

        stream.SetLength(stream.Position);
        return stream.ToArray();
    }

    private static DateTime ParseClockReadResponse(byte[] message)
    {
        const int pduErrOffset = 20;
        const int dtResultOffset = pduErrOffset + 2;
        const int dtLenOffset = dtResultOffset + 2;
        const int dtValueOffset = dtLenOffset + 4;

        var pduErr = Word.FromByteArray(message.Skip(pduErrOffset).Take(2).ToArray());
        if (pduErr != 0)
        {
            throw new Exception($"Response from PLC indicates error 0x{pduErr:X4}.");
        }

        var dtResult = message[dtResultOffset];
        if (dtResult != 0xff)
        {
            throw new Exception($"Response from PLC indicates error 0x{dtResult:X2}.");
        }

        var len = Word.FromByteArray(message.Skip(dtLenOffset).Take(2).ToArray());
        if (len != Types.DateTime.Length)
        {
            throw new Exception($"Unexpected response length {len}, expected {Types.DateTime.Length}.");
        }

        return Types.DateTime.FromByteArray(message.Skip(dtValueOffset).Take(Types.DateTime.Length).ToArray());
    }
}