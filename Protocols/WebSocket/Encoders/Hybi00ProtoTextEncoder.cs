using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Encoders
{
    class Hybi00ProtoTextEncoder : IProtoTextEncoder
    {
        public void EncodeText(IOutputBuffer output, string message)
        {
            var maxByteCount = Encoding.UTF8.GetMaxByteCount(message.Length) + 2;
            var sendBuffer = new byte[maxByteCount];
            sendBuffer[0] = WebSocketConstant.StartByte;
            int bytesCount = Encoding.UTF8.GetBytes(message, 0, message.Length, sendBuffer, 1);
            sendBuffer[1 + bytesCount] = WebSocketConstant.EndByte;
            output.Add(new ArraySegment<byte>(sendBuffer, 0, bytesCount + 2));
        }
    }
}
