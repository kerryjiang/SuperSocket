using System;
using System.Collections.Generic;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.OpHandlers
{
    class CloseHandler : OpHandlerBase
    {
        public CloseHandler(WebSocketServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override sbyte OpCode
        {
            get { return SuperSocket.WebSocket.OpCode.Close; }
        }

        public override WebSocketPackageInfo Handle(IAppSession session, WebSocketContext context, IList<ArraySegment<byte>> data)
        {
            throw new NotImplementedException();
        }
    }
}
