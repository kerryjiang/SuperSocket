using Microsoft.Extensions.Logging;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionListenerFactory
    {
        IConnectionListener CreateConnectionListener<TPackageInfo>(ListenOptions options, ConnectionOptions connectionOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory);
    }
}