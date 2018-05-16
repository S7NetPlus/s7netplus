using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace S7.Net
{

    /// <summary>
    /// Extensions to socket for using awaitable socket operations
    /// </summary>
    public static class SocketExtensions
    {

        /// <summary>
        /// https://blogs.msdn.microsoft.com/pfxteam/2011/12/15/awaiting-socket-operations/
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <returns></returns>
        public static Task<int> ReceiveAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginReceive(buffer, offset, size, socketFlags, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try { t.TrySetResult(s.EndReceive(iar)); }
                catch (Exception exc) { t.TrySetException(exc); }
            }, tcs);
            return tcs.Task;
        }

        /// <summary>
        /// https://blogs.msdn.microsoft.com/pfxteam/2011/12/15/awaiting-socket-operations/
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <returns></returns>
        public static Task<int> SendAsync(
            this Socket socket, byte[] buffer, int offset, int size,
            SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginSend(buffer, offset, size, socketFlags, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try { t.TrySetResult(s.EndReceive(iar)); }
                catch (Exception exc) { t.TrySetException(exc); }
            }, tcs);
            return tcs.Task;
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
}
