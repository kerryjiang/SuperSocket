using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The interface for a request filter to adapt receiving buffer offset
    /// </summary>
    public interface IOffsetAdapter
    {
        /// <summary>
        /// Gets the offset delta.
        /// </summary>
        int OffsetDelta { get; }
    }
}
