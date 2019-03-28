
namespace SuperSocket
{
    public interface IMiddleware
    {
        void Register(IServer server, IAppSession session);

        IMiddleware Next { get; set; }
    }
}