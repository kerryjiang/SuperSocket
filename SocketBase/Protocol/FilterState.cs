using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Filter state enum
    /// </summary>
    public enum FilterState : byte
    {
        /// <summary>
        /// Normal state
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Error state
        /// </summary>
        Error = 1
    }
}
