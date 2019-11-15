
using System.Threading.Tasks;

namespace SuperSocket
{
    public abstract class MiddlewareBase : IMiddleware
    {
        public virtual void Register(IServer server)
        {

        }
        
        public abstract ValueTask<bool> HandleSession(IAppSession session);
    }
}