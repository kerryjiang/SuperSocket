using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides access to the package handling context for a specific package type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPackageHandlingContextAccessor<TPackageInfo> 
    {
        /// <summary>
        /// Gets or sets the package handling context.
        /// </summary>
        PackageHandlingContext<IAppSession, TPackageInfo> PackageHandlingContext { get; set; }
    }
}
