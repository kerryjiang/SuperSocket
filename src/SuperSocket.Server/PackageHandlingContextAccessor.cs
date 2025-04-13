using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides access to the package handling context for a specific package type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class PackageHandlingContextAccessor<TPackageInfo> : IPackageHandlingContextAccessor<TPackageInfo>
    {
        private static AsyncLocal<PackageHandlingContextHolder> AppSessionCurrent { get; set; } = new AsyncLocal<PackageHandlingContextHolder>();

        /// <summary>
        /// Gets or sets the package handling context for the current asynchronous flow.
        /// </summary>
        PackageHandlingContext<IAppSession, TPackageInfo> IPackageHandlingContextAccessor<TPackageInfo>.PackageHandlingContext
        {
            get
            {
                return AppSessionCurrent.Value?.Context;
            }
            set
            {
                var holder = AppSessionCurrent.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    AppSessionCurrent.Value = new PackageHandlingContextHolder { Context = value };
                }
            }
        }

        /// <summary>
        /// Holds the package handling context for the current asynchronous flow.
        /// </summary>
        private class PackageHandlingContextHolder
        {
            /// <summary>
            /// Gets or sets the package handling context.
            /// </summary>
            public PackageHandlingContext<IAppSession, TPackageInfo> Context { get; set; }
        }
    }
}
