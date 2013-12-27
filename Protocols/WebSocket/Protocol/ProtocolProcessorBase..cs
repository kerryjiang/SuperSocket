using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.Protocol
{
    abstract class ProtocolProcessorBase : IProtocolProcessor
    {
        protected ProtocolProcessorBase(int version, ICloseStatusCode closeStatusCode)
        {
            CloseStatusClode = closeStatusCode;
            Version = version;
            VersionTag = version.ToString();
        }

        public abstract bool Handshake(IWebSocketSession session, WebSocketReceiveFilterBase previousFilter, out IReceiveFilter<IWebSocketFragment> dataFrameReader);

        public IProtocolProcessor NextProcessor { get; set; }

        public abstract void SendMessage(IWebSocketSession session, string message);

        public abstract bool TrySendMessage(IWebSocketSession session, string message);

        public abstract void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason);

        public abstract void SendPong(IWebSocketSession session, byte[] pong);

        public abstract void SendPing(IWebSocketSession session, byte[] ping);

        public abstract bool CanSendBinaryData { get; }

        public abstract void SendData(IWebSocketSession session, byte[] data, int offset, int length);

        public abstract bool TrySendData(IWebSocketSession session, byte[] data, int offset, int length);

        public ICloseStatusCode CloseStatusClode { get; private set; }

        public int Version { get; private set; }

        protected string VersionTag { get; private set; }

        public abstract bool IsValidCloseCode(int code);
    }
}
