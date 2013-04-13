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
        /// Collects the server status.
        /// </summary>
        /// <param name="nodeStatus">The node status.</param>
        /// <returns></returns>
        StatusInfoCollection CollectServerStatus(StatusInfoCollection nodeStatus);
    }
}
