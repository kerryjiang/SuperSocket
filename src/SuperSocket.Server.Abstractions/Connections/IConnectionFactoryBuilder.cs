
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionFactoryBuilder
    {
        IConnectionFactory Build<TPackageInfo>(ListenOptions listenOptions, ConnectionOptions connectionOptions);
    }
}