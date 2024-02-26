
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions.Middleware
{
    public interface IMiddleware
    {
        int Order { get; }

        void Start(IServer server);

        void Shutdown(IServer server);

        ValueTask<bool> RegisterSession(IAppSession session);

        ValueTask<bool> UnRegisterSession(IAppSession session);
    }
}