namespace S7.Net.Tcp;

internal class TcpClientWrapperFactory : ITcpClientFactory
{
    public ITcpClient Create()
    {
        return new TcpClientWrapper();
    }
}