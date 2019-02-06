using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IAppSession
    {
        IChannel Channel { get; }

        IServerInfo Server { get; }
    }
}