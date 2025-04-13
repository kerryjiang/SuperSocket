using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server
{
    /// <summary>
    /// Represents the context for handling a package in the server.
    /// </summary>
    /// <typeparam name="TAppSession">The type of the application session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class PackageHandlingContext<TAppSession, TPackageInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageHandlingContext{TAppSession, TPackageInfo}"/> class.
        /// </summary>
        /// <param name="appSession">The application session associated with the package.</param>
        /// <param name="packageInfo">The package information.</param>
        public PackageHandlingContext(TAppSession appSession, TPackageInfo packageInfo)
        {
            AppSession = appSession;
            PackageInfo = packageInfo;
        }

        /// <summary>
        /// Gets the application session associated with the package.
        /// </summary>
        public TAppSession AppSession { get; }

        /// <summary>
        /// Gets the package information.
        /// </summary>
        public TPackageInfo PackageInfo { get; }
    }
}
