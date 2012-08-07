using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// The listener configuration interface
    /// </summary>
    public interface IListenerConfig
    {
        /// <summary>
        /// Gets the ip of listener
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// Gets the port of listener
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the backlog.
        /// </summary>
        int Backlog { get; }

        /// <summary>
        /// Gets the security option, None/Default/Tls/Ssl/...
        /// </summary>
        string Security { get; }
    }
}
