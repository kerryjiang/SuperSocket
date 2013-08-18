using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.WebSocket;

namespace SuperSocket.ServerManager
{
    /// <summary>
    /// Management session
    /// </summary>
    public class ManagementSession : WebSocketSession<ManagementSession>
    {
        /// <summary>
        /// Gets the app server.
        /// </summary>
        public new ManagementServer AppServer
        {
            get { return (ManagementServer)base.AppServer; }
        }

        /// <summary>
        /// Gets a value indicating whether [logged in].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [logged in]; otherwise, <c>false</c>.
        /// </value>
        public bool LoggedIn { get; internal set; }
    }
}
