using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455
    ///  0                   1                   2                   3
    /// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///+-+-+-+-+-------+-+-------------+-------------------------------+
    ///|F|R|R|R| opcode|M| Payload len |    Extended payload length    |
    ///|I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
    ///|N|V|V|V|       |S|             |   (if payload len==126/127)   |
    ///| |1|2|3|       |K|             |                               |
    ///+-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
    ///|     Extended payload length continued, if payload len == 127  |
    ///+ - - - - - - - - - - - - - - - +-------------------------------+
    ///|                               |Masking-key, if MASK set to 1  |
    ///+-------------------------------+-------------------------------+
    ///| Masking-key (continued)       |          Payload Data         |
    ///+-------------------------------- - - - - - - - - - - - - - - - +
    ///:                     Payload Data continued ...                :
    ///+ - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
    ///|                     Payload Data continued ...                |
    ///+---------------------------------------------------------------+
    /// </summary>
    class DraftHybi10ReceiveFilter : FixedHeaderReceiveFilter<WebSocketPackageInfo>, IWebSocketReceiveFilter
    {
        private bool m_Masked;

        private bool m_Final;

        private sbyte m_OpCode;

        private const byte FINAL_FLAG = 0x80;

        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        protected WebSocketContext Context { get; private set; }

        public DraftHybi10ReceiveFilter()
            : base(2)
        {

        }

        public bool Handshake(WebSocketContext context)
        {
            Context = context;

            var handshakeValidator = context.Channel as IHandshakeValidator;
            var request = context.HandshakeRequest;
            var channel = context.Channel;

            if(handshakeValidator != null)
            {
                if(!handshakeValidator.ValidateHandshake(channel, context.HandshakeRequest))
                {
                    channel.Close(CloseReason.ProtocolError);
                    return false;
                }
            }

            var secWebSocketKey = request.Get(WebSocketConstant.SecWebSocketKey);

            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                channel.Close(CloseReason.ProtocolError);
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
                channel.Close(CloseReason.ProtocolError);
                return false;
            }

            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine10);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);
            responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseAcceptLine, secKeyAccept);

            ///TODO: get available sub protocols
            //var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue<string>(WebSocketConstant.SecWebSocketProtocol, string.Empty));

            //if (!string.IsNullOrEmpty(subProtocol))
            //    responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseProtocolLine, subProtocol);

            responseBuilder.AppendWithCrCf();
            byte[] data = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            channel.Send(new ArraySegment<byte>(data));

            return true;
        }

        private int GetPayloadLength(IBufferStream bufferStream, int length)
        {
            if (length == 2)
            {
                var flag = bufferStream.ReadByte();

                m_Final = (FINAL_FLAG & flag) == flag;
                m_OpCode = (sbyte)(flag & 0x08);

                // one byte playload length
                var playloadLen = (int)bufferStream.ReadByte();
                //the highest bit is mask indicator
                m_Masked = playloadLen > 128;
                // remove the mask byte
                playloadLen = playloadLen % 128;

                // no extend playload length
                if (playloadLen < 126)
                {
                    if (!m_Masked)
                        return playloadLen;

                    // masking-key: 4 bytes
                    return playloadLen + 4;
                }

                // playload length takes 2 bytes
                if (playloadLen == 126)
                {
                    ResetSize(4);
                    return -1;
                }
                else// playload length takes 8 bytes
                {
                    ResetSize(10);
                    return -1;
                }
            }
            else if (length == 4)
            {
                bufferStream.Skip(2);

                // 2 bytes
                var playloadLen = bufferStream.ReadUInt16();

                if (m_Masked) // add mask key's length
                    playloadLen += 4;

                return playloadLen;
            }
            else // length = 8
            {
                bufferStream.Skip(2);

                // 8 bytes
                var playloadLen = bufferStream.ReadUInt64();

                if (m_Masked) // add mask key's length
                    playloadLen += 4;

                return (int)playloadLen;
            }
        }

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            var context = Context;

            var payloadLength = GetPayloadLength(bufferStream, length);

            if (payloadLength > 0)
                context.PayloadLength = payloadLength;

            context.OpCode = (OpCode)m_OpCode;

            return payloadLength;
        }

        public override WebSocketPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var context = Context;

            if (!m_Final)
            {
                context.AppendFragment(bufferStream.Buffers);
                return null;
            }

            return context.ResolveLastFragment(bufferStream.Buffers);
        }
    }
}
