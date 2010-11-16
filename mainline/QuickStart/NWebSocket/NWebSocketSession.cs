using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace NWebSocket
{
    public class NWebSocketSession : AppSession<NWebSocketSession, NWebSocketServer, NWebSocketContext>
    {
        protected override void OnClosed()
        {
            
        }

        public override void HandleExceptionalError(Exception e)
        {
            
        }
    }
}
