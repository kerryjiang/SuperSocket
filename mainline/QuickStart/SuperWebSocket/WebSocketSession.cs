using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace SuperWebSocket
{
    public class WebSocketSession : AppSession<WebSocketSession, WebSocketServer, WebSocketContext>
    {
        protected override void OnClosed()
        {
            
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
