using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace S7.Net.UnitTest;

internal static class Responder
{
    private const string Comment = "//";
    private static char[] Space = " ".ToCharArray();

    public static byte[] Respond(RequestResponsePair pair, byte[] request)
    {
        var offset = 0;
        var matches = new Dictionary<string, byte>();
        var res = new List<byte>();
        using var requestReader = new StringReader(pair.RequestPattern);

        string line;
        while ((line = requestReader.ReadLine()) != null)
        {
            var tokens = line.Split(Space, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (token.StartsWith(Comment)) break;

                if (offset >= request.Length)
                {
                    throw new Exception("Request pattern has more data than request.");
                }

                var received = request[offset];

                if (token.Length == 2 && byte.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                {
                    // Number, exact match
                    if (value != received)
                    {
                        throw new Exception($"Incorrect data at offset {offset}. Expected {value:X2}, received {received:X2}.");
                    }
                }
                else
                {
                    matches[token] = received;
                }

                offset++;
            }
        }

        if (offset != request.Length) throw new Exception("Request contained more data than request pattern.");

        using var responseReader = new StringReader(pair.ResponsePattern);
        while ((line = responseReader.ReadLine()) != null)
        {
            var tokens = line.Split(Space, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (token.StartsWith(Comment)) break;

                if (token.Length == 2 && byte.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                {
                    res.Add(value);
                }
                else
                {
                    if (!matches.TryGetValue(token, out var match))
                    {
                        throw new Exception($"Unmatched token '{token}' in response.");
                    }

                    res.Add(match);
                }
            }
        }

        return res.ToArray();
    }
}