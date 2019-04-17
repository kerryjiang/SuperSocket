
namespace SuperSocket
{
    public interface IMiddleware
    {
        void Register(IServer server, IAppSession session);
    }
}