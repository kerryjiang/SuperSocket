
namespace SuperSocket
{
    public abstract class MiddlewareBase : IMiddleware
    {
        public virtual bool AutoRegister { get; } = true;
        
        public abstract void Register(IServer server, IAppSession session);
    }
}