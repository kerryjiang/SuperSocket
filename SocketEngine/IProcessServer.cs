using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// the interface for server instance which works as a process
    /// </summary>
    public interface IProcessServer
    {
        /// <summary>
        /// Gets the process id.
        /// </summary>
        /// <value>
        /// The process id. If the process id is zero, the server instance is not running
        /// </value>
        int ProcessId { get; }
    }
}
