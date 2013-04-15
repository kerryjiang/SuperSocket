using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// The command handling Text fragment
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    class Text<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return OpCode.TextTag;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
        {
            var frame = requestInfo as WebSocketDataFrame;

            if (!CheckFrame(frame))
            {
                session.Close();
                return;
            }

            if (frame.FIN)
            {
                if (session.Frames.Count > 0)
                {
                    session.Close();
                    return;
                }

                var text = GetWebSocketText(frame);
                session.AppServer.OnNewMessageReceived(session, text);
            }
            else
            {
                session.Frames.Add(frame);
            }
        }
    }
}
