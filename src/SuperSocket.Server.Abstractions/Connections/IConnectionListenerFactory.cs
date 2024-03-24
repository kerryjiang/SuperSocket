using Microsoft.Extensions.Logging;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionListenerFactory
    {
        IConnectionListener CreateConnectionListener(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory);
    }
}