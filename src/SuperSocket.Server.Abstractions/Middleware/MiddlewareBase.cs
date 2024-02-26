
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions.Middleware
{
    public abstract class MiddlewareBase : IMiddleware
    {
        public int Order { get; protected set; } = 0;

        public virtual void Start(IServer server)
        {

        }

        public virtual void Shutdown(IServer server)
        {
            
        }
        
        public virtual ValueTask<bool> RegisterSession(IAppSession session)
        {
            return new ValueTask<bool>(true);
        }

        public virtual ValueTask<bool> UnRegisterSession(IAppSession session)
        {
            return new ValueTask<bool>(true);
        }
    }
}