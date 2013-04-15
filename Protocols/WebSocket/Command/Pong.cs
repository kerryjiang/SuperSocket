using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// The command handling Pong
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    class Pong<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return OpCode.PongTag;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
        {
            //Do nothing, last active time has been updated automatically

            var frame = requestInfo as WebSocketDataFrame;

            if (!CheckControlFrame(frame))
            {
                session.Close();
                return;
            }
        }
    }
}
