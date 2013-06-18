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
        /// Gets the server status metadata.
        /// </summary>
        /// <returns></returns>
        StatusInfoAttribute[] GetServerStatusMetadata();

        /// <summary>
        /// Collects the bootstrap status.
        /// </summary>
        /// <param name="bootstrapStatus">The bootstrap status.</param>
        /// <returns></returns>
        StatusInfoCollection CollectServerStatus(StatusInfoCollection bootstrapStatus);
    }
}
