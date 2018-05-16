using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace S7.Net
{
    public static class SocketExtensionsAsync
    {
        public static SocketAwaitable ReceiveAsync(this Socket socket,
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveAsync(awaitable.m_eventArgs))
                awaitable.m_wasCompleted = true;
            return awaitable;
        }

        public static SocketAwaitable SendAsync(this Socket socket,
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.SendAsync(awaitable.m_eventArgs))
                awaitable.m_wasCompleted = true;
            return awaitable;
        }

        public static SocketAwaitable ConnectAsync(this Socket socket, 
            SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ConnectAsync(awaitable.m_eventArgs))
                awaitable.m_wasCompleted = true;
            return awaitable;
        }

        public static async Task<int> ReadAsync(this Socket s, byte[] buffer, int offset, int count)
        {
            // Reusable SocketAsyncEventArgs and awaitable wrapper
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, count);
            var awaitable = new SocketAwaitable(args);

            await s.ReceiveAsync(awaitable);
            return args.BytesTransferred;
        }

        public static async Task<int> SendAsync(this Socket s, byte[] buffer, int offset, int count)
        {
            // Reusable SocketAsyncEventArgs and awaitable wrapper
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, count);
            var awaitable = new SocketAwaitable(args);

            await s.SendAsync(awaitable);
            return args.BytesTransferred;
        }

        /// <summary>
        /// https://blogs.msdn.microsoft.com/pfxteam/2011/12/15/awaiting-socket-operations/
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="addresses"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task<int> ConnectAsync(this Socket socket, System.Net.IPAddress addresses, int port)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginConnect(addresses, port, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try { t.TrySetResult(s.EndReceive(iar)); }
                catch (Exception exc) { t.TrySetException(exc); }
            }, tcs);
            return tcs.Task;
        }
    }

    public sealed class SocketAwaitable : INotifyCompletion
    {
        private readonly static Action SENTINEL = () => { };

        internal bool m_wasCompleted;
        internal Action m_continuation;
        internal SocketAsyncEventArgs m_eventArgs;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            m_eventArgs = eventArgs ?? throw new ArgumentNullException("eventArgs");
            eventArgs.Completed += delegate
            {
                (m_continuation ?? Interlocked.CompareExchange(
                    ref m_continuation, SENTINEL, null))?.Invoke();
            };
        }

        internal void Reset()
        {
            m_wasCompleted = false;
            m_continuation = null;
        }

        public SocketAwaitable GetAwaiter() { return this; }

        public bool IsCompleted { get { return m_wasCompleted; } }

        public void OnCompleted(Action continuation)
        {
            if (m_continuation == SENTINEL ||
                Interlocked.CompareExchange(
                    ref m_continuation, continuation, null) == SENTINEL)
            {
                Task.Run(continuation);
            }
        }

        public void GetResult()
        {
            //if (m_eventArgs.SocketError != SocketError.Success)
            //    throw new SocketException((int)m_eventArgs.SocketError);
        }
    }

}
