using System;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    class WebSocketServiceProvider
    {
        public IBinaryDataParser BinaryDataParser { get; set; }

        public IStringParser StringParser { get; set; }
    }
}
