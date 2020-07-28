using Super.Engine;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Super.Host
{
    [ExposeServices(typeof(IOnlineSession))]
    public class OnlineSession : AppSession, IOnlineSession, ITransientDependency
    {      
        public IPackageEncoder<OnlinePackageInfo> PackageEncoder { get; set; }
     
        public SuperSocketService<OnlinePackageInfo> OnlineServer => Server.As<SuperSocketService<OnlinePackageInfo>>();
     
        ValueTask IOnlineSession.SendAsync(OnlinePackageInfo packageInfo) => this.As<IAppSession>().SendAsync(PackageEncoder, packageInfo);
    }
}
