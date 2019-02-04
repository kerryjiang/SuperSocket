using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IAppSession
    {
        IChannelBase Channel { get; }

        IServerInfo Server { get; }
    }
}