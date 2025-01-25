using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    public interface IServer : IServerInfo, IDisposable, IAsyncDisposable
    {
        Task<bool> StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}