using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace S7.Net
{
    internal class SocketClient
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

        public SocketClient(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(addressFamily, socketType, protocolType);
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

        public void SetReceiveTimeout(int milis)
        {
            _receiveTimeout = milis;
        }

        public void SetSendTimeout(int milis)
        {
            _sendTimeout = milis;
        }

        public int Send(byte[] buffer, int size)
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

        public int Receive(byte[] buffer, int size)
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

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Dispose();
                _socket = null;
            }
        }

        private Socket _socket = null;
        private int _receiveTimeout = TIMEOUT_MILLISECONDS;
        private int _sendTimeout = TIMEOUT_MILLISECONDS;

        private readonly static ManualResetEvent _clientDone =
            new ManualResetEvent(false);

        private const int TIMEOUT_MILLISECONDS = 1000;

    }
}