using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Command
{
    public class CommandMiddleware<TKey, TPackageInfo> : MiddlewareBase
        where TPackageInfo : IKeyedPackageInfo<TKey>
    {
        public override void Register(IServer server, IAppSession session)
        {
            var channel = session.Channel as IChannel<TPackageInfo>;
            
            if (channel == null)
                throw new Exception("Unmatched package type.");
            
            channel.PackageReceived += OnPackageReceived;
        }

        private async Task OnPackageReceived(IChannel channel, TPackageInfo package)
        {
            await Task.Delay(0);
        }
    }
}
