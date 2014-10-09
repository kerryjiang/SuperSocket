using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.OpHandlers
{
    class TextHandler : OpHandlerBase
    {
        public TextHandler(WebSocketServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override sbyte OpCode
        {
            get { return SuperSocket.WebSocket.OpCode.Text; }
        }

        public override WebSocketPackageInfo Handle(IAppSession session, WebSocketContext context, IList<ArraySegment<byte>> data)
        {
            var total = data.Sum(d => d.Count);
            var payloadLength = context.PayloadLength;
            var text = Encoding.UTF8.GetString(data, total - payloadLength, payloadLength);
            return new WebSocketPackageInfo(text, ServiceProvider.StringParser);
        }
    }
}
