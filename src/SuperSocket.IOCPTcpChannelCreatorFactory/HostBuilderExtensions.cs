namespace SuperSocket.IOCPTcpChannelCreatorFactory;

public static class HostBuilderExtensions
{
    public static ISuperSocketHostBuilder UseIOCPTcpChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        return hostBuilder.UseChannelCreatorFactory<TcpIocpChannelCreatorFactory>();
    }
}