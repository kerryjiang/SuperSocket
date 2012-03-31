using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Session status enum
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Healthy, connected
        /// </summary>
        Healthy,

        /// <summary>
        /// Error state, but connected
        /// </summary>
        Error,

        /// <summary>
        /// Disconnected
        /// </summary>
        Disconnected
    }
}
