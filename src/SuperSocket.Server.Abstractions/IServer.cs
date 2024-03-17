using System;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    public interface IServer : IServerInfo, IDisposable, IAsyncDisposable
    {
        Task<bool> StartAsync();

        Task StopAsync();
    }
}