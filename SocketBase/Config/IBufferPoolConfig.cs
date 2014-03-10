using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Buffer pool configuration interface
    /// </summary>
    public interface IBufferPoolConfig
    {
        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        /// <value>
        /// The size of the buffer.
        /// </value>
        int BufferSize { get; }

        /// <summary>
        /// Gets the initial count.
        /// </summary>
        /// <value>
        /// The initial count.
        /// </value>
        int InitialCount { get; }
    }
}
