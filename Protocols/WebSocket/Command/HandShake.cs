using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// The command handle handshake request
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    class HandShake<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return OpCode.HandshakeTag;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
        {
            session.OnHandshakeSuccess();
        }
    }
}
