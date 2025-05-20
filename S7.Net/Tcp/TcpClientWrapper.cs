using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace S7.Net.Tcp;

internal class TcpClientWrapper : TcpClient, ITcpClient
{
    public async Task ConnectAsync(string ip, int port, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        await base.ConnectAsync(ip, port, cancellationToken).ConfigureAwait(false);
#else
        await base.ConnectAsync(ip, port).ConfigureAwait(false);
#endif
    }

    public void Close()
    {
#if NET20_OR_GREATER
        base.Close();
#endif
    }
}