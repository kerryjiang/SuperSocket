using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Command
{
    /// <summary>
    /// The command handling continuation fragment
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    class Continuation<TWebSocketSession> : FragmentCommand<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return OpCode.ContinuationTag;
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

            session.Frames.Add(frame);

            if (!frame.FIN)
            {
                return;
            }

            var firstFrame = session.Frames[0];

            if (firstFrame.OpCode == OpCode.Text)
            {
                var text = GetWebSocketText(session.Frames);
                session.Frames.Clear();
                session.AppServer.OnNewMessageReceived(session, text);
            }
            else if (firstFrame.OpCode == OpCode.Binary)
            {
                var data = GetWebSocketData(session.Frames);
                session.Frames.Clear();
                session.AppServer.OnNewDataReceived(session, data);
            }
            else
            {
                //http://tools.ietf.org/html/rfc6455#section-5.5
                //All control frames MUST have a payload length of 125 bytes or less and MUST NOT be fragmented.
                session.Close();
            }
        }
    }
}
