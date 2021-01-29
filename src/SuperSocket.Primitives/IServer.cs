using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IServer : IServerInfo, IDisposable, IAsyncDisposable
    {
        Task<bool> StartAsync();

        Task StopAsync();
    }
}