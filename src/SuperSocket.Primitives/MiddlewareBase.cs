
namespace SuperSocket
{
    public abstract class MiddlewareBase : IMiddleware
    {
        public abstract void Register(IServer server, IAppSession session);

        public IMiddleware Next { get; set; }
    }
}