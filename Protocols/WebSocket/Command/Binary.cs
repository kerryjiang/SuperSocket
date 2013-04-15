using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// The command handling binary data
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    class Binary<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return OpCode.BinaryTag;
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

                var data = GetWebSocketData(frame);
                session.AppServer.OnNewDataReceived(session, data);
            }
            else
            {
                session.Frames.Add(frame);
            }
        }
    }
}
