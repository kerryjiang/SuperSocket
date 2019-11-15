
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IMiddleware
    {
        void Register(IServer server);
        

        ValueTask<bool> HandleSession(IAppSession session);
    }
}