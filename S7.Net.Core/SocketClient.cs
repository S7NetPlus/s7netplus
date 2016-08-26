using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace S7.Net
{
    /// <summary>
    /// This class encapsulate System.Net.Sockets.Socket class of .Net core, so we can use the same methods of the standard Socket class inside the S7.Net sources.
    /// </summary>
    internal class Socket
    {

        public bool Connected
        {
            get
            {
                if (_socket == null)
                    return false;

                return _socket.Connected;
            }
        }

        public SocketError LastSocketError { get; private set; }

        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new System.Net.Sockets.Socket(addressFamily, socketType, protocolType);
        }

        public void Connect(IPEndPoint server)
        {
            if (Connected)
                return;

            LastSocketError = SocketError.NotConnected;

            var socketEventArg = new SocketAsyncEventArgs();

            socketEventArg.RemoteEndPoint = server;

            var completedEvent = new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                LastSocketError = e.SocketError;
                _clientDone.Set();
            });

            socketEventArg.Completed += completedEvent;

            _clientDone.Reset();

            LastSocketError = SocketError.TimedOut;

            _socket.ConnectAsync(socketEventArg);

            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            socketEventArg.Completed -= completedEvent;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlag)
        {
            var response = 0;

            if (_socket != null)
            {
                var socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                var completedEvent = new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    LastSocketError = e.SocketError;

                    if (e.SocketError == SocketError.Success)
                        response = e.BytesTransferred;

                    _clientDone.Set();
                });

                socketEventArg.Completed += completedEvent;

                socketEventArg.SetBuffer(buffer, 0, size);

                _clientDone.Reset();

                LastSocketError = SocketError.TimedOut;

                _socket.SendAsync(socketEventArg);

                _clientDone.WaitOne(_sendTimeout);

                socketEventArg.Completed -= completedEvent;
            }
            else
            {
                LastSocketError = SocketError.NotInitialized;
            }

            return response;
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlag)
        {
            var response = 0;

            if (_socket != null)
            {
                var socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.SetBuffer(buffer, 0, size);

                var completedEvent = new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    LastSocketError = e.SocketError;

                    if (e.SocketError == SocketError.Success)
                        response = e.BytesTransferred;

                    _clientDone.Set();
                });

                socketEventArg.Completed += completedEvent;

                _clientDone.Reset();

                LastSocketError = SocketError.TimedOut;

                _socket.ReceiveAsync(socketEventArg);

                _clientDone.WaitOne(_receiveTimeout);

                socketEventArg.Completed -= completedEvent;
            }
            else
            {
                LastSocketError = SocketError.NotInitialized;
            }

            return response;
        }

        public void Shutdown(SocketShutdown how)
        {
            _socket.Shutdown(how);
        }

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        //
        // Summary:
        //     Sets the specified System.Net.Sockets.Socket option to the specified integer
        //     value.
        //
        // Parameters:
        //   optionLevel:
        //     One of the System.Net.Sockets.SocketOptionLevel values.
        //
        //   optionName:
        //     One of the System.Net.Sockets.SocketOptionName values.
        //
        //   optionValue:
        //     A value of the option.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            switch (optionName)
            {
                case SocketOptionName.ReceiveTimeout:
                    _receiveTimeout = optionValue;
                    break;

                case SocketOptionName.SendTimeout:
                    _sendTimeout = optionValue;
                    break;

                default:
                    throw new NotImplementedException("SetSocketOption option not implemented");
            }
        }

        private System.Net.Sockets.Socket _socket = null;
        private int _receiveTimeout = TIMEOUT_MILLISECONDS;
        private int _sendTimeout = TIMEOUT_MILLISECONDS;

        private readonly static ManualResetEvent _clientDone =
            new ManualResetEvent(false);

        private const int TIMEOUT_MILLISECONDS = 1000;

    }

    //
    // Summary:
    //     Specifies socket send and receive behaviors.
    [Flags]
    public enum SocketFlags
    {
        //
        // Summary:
        //     Use no flags for this call.
        None = 0,
        ////
        //// Summary:
        ////     Process out-of-band data.
        //OutOfBand = 1,
        ////
        //// Summary:
        ////     Peek at the incoming message.
        //Peek = 2,
        ////
        //// Summary:
        ////     Send without using routing tables.
        //DontRoute = 4,
        ////
        //// Summary:
        ////     Provides a standard value for the number of WSABUF structures that are used to
        ////     send and receive data.
        //MaxIOVectorLength = 16,
        ////
        //// Summary:
        ////     The message was too large to fit into the specified buffer and was truncated.
        //Truncated = 256,
        ////
        //// Summary:
        ////     Indicates that the control data did not fit into an internal 64-KB buffer and
        ////     was truncated.
        //ControlDataTruncated = 512,
        ////
        //// Summary:
        ////     Indicates a broadcast packet.
        //Broadcast = 1024,
        ////
        //// Summary:
        ////     Indicates a multicast packet.
        //Multicast = 2048,
        ////
        //// Summary:
        ////     Partial send or receive for message.
        //Partial = 32768
    }

    //
    // Summary:
    //     Defines socket option levels for the System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Int32)
    //     and System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName)
    //     methods.
    public enum SocketOptionLevel
    {
        //
        // Summary:
        //     System.Net.Sockets.Socket options apply only to IP sockets.
        IP = 0,
        //
        // Summary:
        //     System.Net.Sockets.Socket options apply only to TCP sockets.
        Tcp = 6,
        //
        // Summary:
        //     System.Net.Sockets.Socket options apply only to UDP sockets.
        Udp = 17,
        //
        // Summary:
        //     System.Net.Sockets.Socket options apply only to IPv6 sockets.
        IPv6 = 41,
        //
        // Summary:
        //     System.Net.Sockets.Socket options apply to all sockets.
        Socket = 65535
    }

    //
    // Summary:
    //     Defines configuration option names.
    public enum SocketOptionName
    {
        //
        // Summary:
        //     Close the socket gracefully without lingering.
        DontLinger = -129,
        //
        // Summary:
        //     Enables a socket to be bound for exclusive access.
        ExclusiveAddressUse = -5,
        //
        // Summary:
        //     Record debugging information.
        Debug = 1,
        //
        // Summary:
        //     Specifies the IP options to be inserted into outgoing datagrams.
        IPOptions = 1,
        //
        // Summary:
        //     Disables the Nagle algorithm for send coalescing.
        NoDelay = 1,
        //
        // Summary:
        //     Send UDP datagrams with checksum set to zero.
        NoChecksum = 1,
        //
        // Summary:
        //     The socket is listening.
        AcceptConnection = 2,
        //
        // Summary:
        //     Indicates that the application provides the IP header for outgoing datagrams.
        HeaderIncluded = 2,
        //
        // Summary:
        //     Use urgent data as defined in RFC-1222. This option can be set only once; after
        //     it is set, it cannot be turned off.
        BsdUrgent = 2,
        //
        // Summary:
        //     Use expedited data as defined in RFC-1222. This option can be set only once;
        //     after it is set, it cannot be turned off.
        Expedited = 2,
        //
        // Summary:
        //     Change the IP header type of the service field.
        TypeOfService = 3,
        //
        // Summary:
        //     Allows the socket to be bound to an address that is already in use.
        ReuseAddress = 4,
        //
        // Summary:
        //     Set the IP header Time-to-Live field.
        IpTimeToLive = 4,
        //
        // Summary:
        //     Use keep-alives.
        KeepAlive = 8,
        //
        // Summary:
        //     Set the interface for outgoing multicast packets.
        MulticastInterface = 9,
        //
        // Summary:
        //     An IP multicast Time to Live.
        MulticastTimeToLive = 10,
        //
        // Summary:
        //     An IP multicast loopback.
        MulticastLoopback = 11,
        //
        // Summary:
        //     Add an IP group membership.
        AddMembership = 12,
        //
        // Summary:
        //     Drop an IP group membership.
        DropMembership = 13,
        //
        // Summary:
        //     Do not fragment IP datagrams.
        DontFragment = 14,
        //
        // Summary:
        //     Join a source group.
        AddSourceMembership = 15,
        //
        // Summary:
        //     Do not route; send the packet directly to the interface addresses.
        DontRoute = 16,
        //
        // Summary:
        //     Drop a source group.
        DropSourceMembership = 16,
        //
        // Summary:
        //     Block data from a source.
        BlockSource = 17,
        //
        // Summary:
        //     Unblock a previously blocked source.
        UnblockSource = 18,
        //
        // Summary:
        //     Return information about received packets.
        PacketInformation = 19,
        //
        // Summary:
        //     Set or get the UDP checksum coverage.
        ChecksumCoverage = 20,
        //
        // Summary:
        //     Specifies the maximum number of router hops for an Internet Protocol version
        //     6 (IPv6) packet. This is similar to Time to Live (TTL) for Internet Protocol
        //     version 4.
        HopLimit = 21,
        //
        // Summary:
        //     Permit sending broadcast messages on the socket.
        Broadcast = 32,
        //
        // Summary:
        //     Bypass hardware when possible.
        UseLoopback = 64,
        //
        // Summary:
        //     Linger on close if unsent data is present.
        Linger = 128,
        //
        // Summary:
        //     Receives out-of-band data in the normal data stream.
        OutOfBandInline = 256,
        //
        // Summary:
        //     Specifies the total per-socket buffer space reserved for sends. This is unrelated
        //     to the maximum message size or the size of a TCP window.
        SendBuffer = 4097,
        //
        // Summary:
        //     Specifies the total per-socket buffer space reserved for receives. This is unrelated
        //     to the maximum message size or the size of a TCP window.
        ReceiveBuffer = 4098,
        //
        // Summary:
        //     Specifies the low water mark for Overload:System.Net.Sockets.Socket.Send operations.
        SendLowWater = 4099,
        //
        // Summary:
        //     Specifies the low water mark for Overload:System.Net.Sockets.Socket.Receive operations.
        ReceiveLowWater = 4100,
        //
        // Summary:
        //     Send a time-out. This option applies only to synchronous methods; it has no effect
        //     on asynchronous methods such as the System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     method.
        SendTimeout = 4101,
        //
        // Summary:
        //     Receive a time-out. This option applies only to synchronous methods; it has no
        //     effect on asynchronous methods such as the System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     method.
        ReceiveTimeout = 4102,
        //
        // Summary:
        //     Get the error status and clear.
        Error = 4103,
        //
        // Summary:
        //     Get the socket type.
        Type = 4104,
        //
        // Summary:
        //     Updates an accepted socket's properties by using those of an existing socket.
        //     This is equivalent to using the Winsock2 SO_UPDATE_ACCEPT_CONTEXT socket option
        //     and is supported only on connection-oriented sockets.
        UpdateAcceptContext = 28683,
        //
        // Summary:
        //     Updates a connected socket's properties by using those of an existing socket.
        //     This is equivalent to using the Winsock2 SO_UPDATE_CONNECT_CONTEXT socket option
        //     and is supported only on connection-oriented sockets.
        UpdateConnectContext = 28688,
        //
        // Summary:
        //     Not supported; will throw a System.Net.Sockets.SocketException if used.
        MaxConnections = int.MaxValue
    }
}