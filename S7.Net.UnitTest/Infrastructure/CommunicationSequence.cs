using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace S7.Net.UnitTest;

internal class CommunicationSequence : IEnumerable<RequestResponsePair>
{
    private readonly List<RequestResponsePair> _requestResponsePairs = new List<RequestResponsePair>();

    public void Add(RequestResponsePair requestResponsePair)
    {
        _requestResponsePairs.Add(requestResponsePair);
    }

    public void Add(string requestPattern, string responsePattern)
    {
        _requestResponsePairs.Add(new RequestResponsePair(requestPattern, responsePattern));
    }

    public IEnumerator<RequestResponsePair> GetEnumerator()
    {
        return _requestResponsePairs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Task Serve(out int port)
    {
        var socket = CreateBoundListenSocket(out port);
        socket.Listen(0);

        async Task Impl()
        {
            await Task.Yield();
            var socketIn = socket.Accept();

            var buffer = ArrayPool<byte>.Shared.Rent(1024);
            try
            {
                foreach (var pair in _requestResponsePairs)
                {
                    var bytesReceived = socketIn.Receive(buffer, SocketFlags.None);

                    var received = buffer.Take(bytesReceived).ToArray();
                    Console.WriteLine($"=> {BitConverter.ToString(received)}");

                    var response = Responder.Respond(pair, received);

                    Console.WriteLine($"<= {BitConverter.ToString(response)}");
                    socketIn.Send(response);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            socketIn.Close();
        }

        return Impl();
    }

    private static Socket CreateBoundListenSocket(out int port)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endpoint = new IPEndPoint(IPAddress.Loopback, 0);

        socket.Bind(endpoint);

        var localEndpoint = (IPEndPoint)socket.LocalEndPoint!;
        port = localEndpoint.Port;

        return socket;
    }
}
