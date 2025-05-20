using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace S7.Net.Tcp
{
    public interface ITcpClient
    {
        public int ReceiveTimeout { get; set; }

        public int SendTimeout { get; set; }

        public bool Connected { get; }

        public void Close();

        public Task ConnectAsync(string ip, int port, CancellationToken cancellationToken = default);

        public NetworkStream GetStream();
    }
}
