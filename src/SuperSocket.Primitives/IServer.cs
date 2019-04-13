using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IServer : IServerInfo
    {
        Task<bool> StartAsync();

        Task StopAsync();

        int SessionCount { get; }

        void UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware;
    }
}