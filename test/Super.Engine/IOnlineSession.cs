using SuperSocket;
using System.Threading.Tasks;

namespace Super.Engine
{
    public interface IOnlineSession : IAppSession
    {
        ValueTask SendAsync(OnlinePackageInfo packageInfo);
    }
}
