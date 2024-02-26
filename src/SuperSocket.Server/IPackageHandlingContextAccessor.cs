using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public interface IPackageHandlingContextAccessor<TPackageInfo> 
    {
        PackageHandlingContext<IAppSession, TPackageInfo> PackageHandlingContext { get; set; }
    }
}
