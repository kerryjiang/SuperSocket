using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public delegate void PackageHandler<TReceivePackage>(EasyClient<TReceivePackage> sender, TReceivePackage package)
        where TReceivePackage : class;
}