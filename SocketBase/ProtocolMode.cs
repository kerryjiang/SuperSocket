using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Application protocol mode
    /// </summary>
    public enum ProtocolMode
    {
        /// <summary>
        /// The command line protocol
        /// </summary>
        CommandLine,
        /// <summary>
        /// The web socket protocol
        /// </summary>
        WebSocket,
        /// <summary>
        /// The custom protocol defined by yourself
        /// </summary>
        Custom
    }
}
