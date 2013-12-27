using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455#section-4.4
    /// </summary>
    class MultipleProtocolSwitchProcessor : IProtocolProcessor
    {
        private byte[] m_SwitchResponse;

        public MultipleProtocolSwitchProcessor(int[] availableVersions)
        {
            var responseBuilder = new StringBuilder();

            responseBuilder.AppendWithCrCf("HTTP/1.1 400 Bad Request");
            responseBuilder.AppendWithCrCf("Upgrade: WebSocket");
            responseBuilder.AppendWithCrCf("Connection: Upgrade");
            responseBuilder.AppendWithCrCf("Sec-WebSocket-Version: " + string.Join(", ", availableVersions.Select(i => i.ToString()).ToArray()));
            responseBuilder.AppendWithCrCf();

            m_SwitchResponse = Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        public bool CanSendBinaryData { get { return false; } }

        public ICloseStatusCode CloseStatusClode { get; set; }

        public IProtocolProcessor NextProcessor { get; set; }

        public bool Handshake(IWebSocketSession session, WebSocketReceiveFilterBase previousReader, out IReceiveFilter<IWebSocketFragment> dataFrameReader)
        {
            dataFrameReader = null;
            session.SendRawData(m_SwitchResponse, 0, m_SwitchResponse.Length);
            return true;
        }

        public void SendMessage(IWebSocketSession session, string message)
        {
            throw new NotImplementedException();
        }

        public void SendData(IWebSocketSession session, byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason)
        {
            throw new NotImplementedException();
        }

        public void SendPong(IWebSocketSession session, byte[] pong)
        {
            throw new NotImplementedException();
        }

        public void SendPing(IWebSocketSession session, byte[] ping)
        {
            throw new NotImplementedException();
        }

        public int Version
        {
            get { return 0; }
        }

        public bool IsValidCloseCode(int code)
        {
            throw new NotImplementedException();
        }

        public bool TrySendMessage(IWebSocketSession session, string message)
        {
            throw new NotImplementedException();
        }

        public bool TrySendData(IWebSocketSession session, byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }
    }
}
