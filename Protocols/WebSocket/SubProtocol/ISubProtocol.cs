using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Config;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// SubProtocol interface
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public interface ISubProtocol<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Initializes with the specified config.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="protocolConfig">The protocol config.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        bool Initialize(IAppServer appServer, SubProtocolConfig protocolConfig, ILog logger);

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Gets the sub request parser.
        /// </summary>
        IRequestInfoParser<SubRequestInfo> SubRequestParser { get; }

        /// <summary>
        /// Tries the get command.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command);
    }
}
