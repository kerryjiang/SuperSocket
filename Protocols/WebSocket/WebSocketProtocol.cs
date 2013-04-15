using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Protocol;
using SuperSocket.WebSocket.SubProtocol;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket protocol
    /// </summary>
    public class WebSocketProtocol : IReceiveFilterFactory<IWebSocketFragment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketProtocol"/> class.
        /// </summary>
        public WebSocketProtocol()
        {

        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<IWebSocketFragment> CreateFilter(IAppServer appServer, IAppSession appSession, System.Net.IPEndPoint remoteEndPoint)
        {
            return new WebSocketHeaderReceiveFilter((IWebSocketSession)appSession);
        }
    }
}
