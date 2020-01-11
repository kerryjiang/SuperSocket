
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IMiddleware
    {
        void Start(IServer server);

        void Shutdown(IServer server);

        ValueTask<bool> RegisterSession(IAppSession session);
    }
}