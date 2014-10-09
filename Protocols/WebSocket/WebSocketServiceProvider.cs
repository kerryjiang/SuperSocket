using System;
using System.Linq;
using System.Collections.Generic;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.OpHandlers;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket
{
    class WebSocketServiceProvider
    {
        public IBinaryDataParser BinaryDataParser { get; private set; }

        public IStringParser StringParser { get; private set; }

        public Dictionary<sbyte, IOpHandler> OpHandlers { get; private set; }

        public WebSocketServiceProvider(IAppServer appServer)
        {
            BinaryDataParser = appServer.GetService<IBinaryDataParser>();
            StringParser = appServer.GetService<IStringParser>();
            OpHandlers = (new IOpHandler[] {
                            new TextHandler(this), new BinaryHandler(this),
                            new PingHandler(this), new PongHandler(this),
                            new CloseHandler(this) })
                         .ToDictionary(h => h.OpCode);
        }
    }
}
