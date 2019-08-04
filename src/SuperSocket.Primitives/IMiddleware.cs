
namespace SuperSocket
{
    public interface IMiddleware
    {
        bool AutoRegister { get; }

        void Register(IServer server, IAppSession session);
    }
}