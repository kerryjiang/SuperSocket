using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// StatusInfo source interface
    /// </summary>
    public interface IStatusInfoSource
    {
        /// <summary>
        /// Collects the bootstrap status.
        /// </summary>
        /// <param name="bootstrapStatus">The bootstrap status.</param>
        /// <returns></returns>
        StatusInfoCollection CollectServerStatus(StatusInfoCollection bootstrapStatus);
    }

    /// <summary>
    /// Server metadata provder interface
    /// </summary>
    public interface IServerMetadataProvider
    {
        /// <summary>
        /// Gets the application server metadata.
        /// </summary>
        /// <returns></returns>
        AppServerMetadata GetAppServerMetadata();
    }
}
