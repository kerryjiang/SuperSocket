
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionFactoryBuilder
    {
        IConnectionFactory Build(ListenOptions listenOptions, ConnectionOptions connectionOptions);
    }
}