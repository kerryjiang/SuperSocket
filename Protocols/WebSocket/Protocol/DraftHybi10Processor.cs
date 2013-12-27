using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10
    /// </summary>
    class DraftHybi10Processor : ProtocolProcessorBase
    {
        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        protected DraftHybi10Processor(int version, ICloseStatusCode closeStatusCode)
            : base(version, closeStatusCode)
        {

        }

        public DraftHybi10Processor()
            : base(8, new CloseStatusCodeHybi10())
        {

        }

        private const string m_OriginKey = "Sec-WebSocket-Origin";

        protected virtual string OriginKey
        {
            get { return m_OriginKey; }
        }

        public override bool Handshake(IWebSocketSession session, WebSocketReceiveFilterBase previousFilter, out IReceiveFilter<IWebSocketFragment> dataFrameReader)
        {
            if (!VersionTag.Equals(session.SecWebSocketVersion) && NextProcessor != null)
            {
                return NextProcessor.Handshake(session, previousFilter, out dataFrameReader);
            }

            dataFrameReader = null;

            session.ProtocolProcessor = this;

            if (!session.AppServer.ValidateHandshake(session, session.Items.GetValue<string>(OriginKey, string.Empty)))
                return false;

            var secWebSocketKey = session.Items.GetValue<string>(WebSocketConstant.SecWebSocketKey, string.Empty);

            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }

            var responseBuilder = new StringBuilder();

            string secKeyAccept = string.Empty;

            try
            {
                secKeyAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + m_Magic)));
            }
            catch (Exception)
            {
                return false;
            }

            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine10);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);
            responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseAcceptLine, secKeyAccept);

            var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty));

            if (!string.IsNullOrEmpty(subProtocol))
                responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseProtocolLine, subProtocol);

            responseBuilder.AppendWithCrCf();
            byte[] data = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            session.SendRawData(data, 0, data.Length);

            dataFrameReader = new WebSocketDataFrameReceiveFilter();

            return true;
        }

        public override bool CanSendBinaryData
        {
            get { return true; }
        }

        public override void SendData(IWebSocketSession session, byte[] data, int offset, int length)
        {
            SendPackage(session, OpCode.Binary, data, offset, length);
        }

        public override void SendMessage(IWebSocketSession session, string message)
        {
            SendMessage(session, OpCode.Text, message);
        }

        public override void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason)
        {
            byte[] playloadData = new byte[(string.IsNullOrEmpty(closeReason) ? 0 : Encoding.UTF8.GetMaxByteCount(closeReason.Length)) + 2];

            int highByte = statusCode / 256;
            int lowByte = statusCode % 256;

            playloadData[0] = (byte)highByte;
            playloadData[1] = (byte)lowByte;

            var playloadLength = playloadData.Length;

            if (!string.IsNullOrEmpty(closeReason))
            {
                int bytesCount = Encoding.UTF8.GetBytes(closeReason, 0, closeReason.Length, playloadData, 2);
                playloadLength = bytesCount + 2;
            }

            SendPackage(session, OpCode.Close, playloadData, 0, playloadLength);
        }

        public override void SendPong(IWebSocketSession session, byte[] pong)
        {
            SendPackage(session, OpCode.Pong, pong, 0, pong.Length);
        }

        public override void SendPing(IWebSocketSession session, byte[] ping)
        {
            SendPackage(session, OpCode.Ping, ping, 0, ping.Length);
        }

        private byte[] GetPackageData(int opCode, byte[] data, int offset, int length)
        {
            byte[] fragment;

            if (length < 126)
            {
                fragment = new byte[2 + length];
                fragment[1] = (byte)length;
            }
            else if (length < 65536)
            {
                fragment = new byte[4 + length];
                fragment[1] = (byte)126;
                fragment[2] = (byte)(length / 256);
                fragment[3] = (byte)(length % 256);
            }
            else
            {
                fragment = new byte[10 + length];
                fragment[1] = (byte)127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    fragment[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }

            fragment[0] = (byte)(opCode | 0x80);

            if (length > 0)
            {
                Buffer.BlockCopy(data, offset, fragment, fragment.Length - length, length);
            }

            return fragment;
        }

        private void SendPackage(IWebSocketSession session, int opCode, byte[] data, int offset, int length)
        {
            var fragment = GetPackageData(opCode, data, offset, length);
            session.SendRawData(fragment, 0, fragment.Length);
        }

        private bool TrySendPackage(IWebSocketSession session, int opCode, byte[] data, int offset, int length)
        {
            var fragment = GetPackageData(opCode, data, offset, length);
            return session.TrySendRawData(fragment, 0, fragment.Length);
        }

        private void SendMessage(IWebSocketSession session, int opCode, string message)
        {
            byte[] playloadData = Encoding.UTF8.GetBytes(message);
            SendPackage(session, opCode, playloadData, 0, playloadData.Length);
        }

        private bool TrySendMessage(IWebSocketSession session, int opCode, string message)
        {
            byte[] playloadData = Encoding.UTF8.GetBytes(message);
            return TrySendPackage(session, opCode, playloadData, 0, playloadData.Length);
        }

        public override bool IsValidCloseCode(int code)
        {
            var closeCode = this.CloseStatusClode;

            if (code >= 0 && code <= 999)
                return false;

            if (code >= 1000 && code <= 1999)
            {
                if (code == closeCode.NormalClosure
                    || code == closeCode.GoingAway
                    || code == closeCode.ProtocolError
                    || code == closeCode.NotAcceptableData
                    || code == closeCode.TooLargeFrame
                    || code == closeCode.InvalidUTF8)
                {
                    return true;
                }

                return false;
            }

            if (code >= 2000 && code <= 4999)
                return true;

            return false;
        }

        public override bool TrySendMessage(IWebSocketSession session, string message)
        {
            return TrySendMessage(session, OpCode.Text, message);
        }

        public override bool TrySendData(IWebSocketSession session, byte[] data, int offset, int length)
        {
            return TrySendPackage(session, OpCode.Binary, data, offset, length);
        }
    }
}