using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IServer : IServerInfo
    {
        Task<bool> StartAsync();

        Task StopAsync();

        void Use<TMiddleware>() where TMiddleware : IMiddleware;
    }
}