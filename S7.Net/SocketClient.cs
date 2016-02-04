using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace S7.Net
{
    internal class SocketClient
    {

        public bool Connected { get; private set; }

        public SocketClient(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(addressFamily, socketType, protocolType);
        }

        public void Connect(IPEndPoint server)
        {
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

            socketEventArg.RemoteEndPoint = server;

            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Connected = true;
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }

                _clientDone.Set();
            });

            _clientDone.Reset();

            _socket.ConnectAsync(socketEventArg);

            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
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
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        response = e.BytesTransferred;
                    }
                    else
                    {
                        throw new SocketException((int)e.SocketError);
                    }

                    _clientDone.Set();
                });

                socketEventArg.SetBuffer(buffer, 0, size);

                _clientDone.Reset();

                _socket.SendAsync(socketEventArg);

                _clientDone.WaitOne(_sendTimeout);
            }
            else
            {
                throw new SocketException((int)SocketError.NotInitialized);
            }

            return response;
        }

        public int Receive(byte[] buffer, int size)
        {
            var response = 0;

            if (_socket != null)
            {
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.SetBuffer(buffer, 0, size);

                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        response = e.BytesTransferred;
                    }
                    else
                    {
                        throw new SocketException((int)e.SocketError);
                    }

                    _clientDone.Set();
                });

                _clientDone.Reset();

                _socket.ReceiveAsync(socketEventArg);

                _clientDone.WaitOne(_receiveTimeout);
            }
            else
            {
                throw new SocketException((int)SocketError.NotInitialized);
            }

            return response;
        }

        public void Close()
        {
            Connected = false;

            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
        }

        private Socket _socket = null;
        private int _receiveTimeout = TIMEOUT_MILLISECONDS;
        private int _sendTimeout = TIMEOUT_MILLISECONDS;

        private static ManualResetEvent _clientDone = new ManualResetEvent(false);

        private const int TIMEOUT_MILLISECONDS = 5000;

    }
}