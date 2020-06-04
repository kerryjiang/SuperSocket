using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.WebSocket;
using SuperSocket.ProtoBase;
using System.Buffers;

namespace WebSocketServer
{
    public class MyWebsocketEncoder : IPackageEncoder<WebSocketPackage>
    {
        private WebSocketEncoder WebSocketEncoder { get; }
        public MyWebsocketEncoder()
        {
            WebSocketEncoder = new WebSocketEncoder();
        }
        public int Encode(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            var message = new WebSocketMessage()
            {
                Data = pack.Data,
                Message = pack.Message,
                OpCode = pack.OpCode
            };
            return WebSocketEncoder.Encode(writer, message);
        }
    }
}
