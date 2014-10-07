using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using System.IO;

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
    class DraftHybi10ReceiveFilter : FixedHeaderReceiveFilter<StringPackageInfo>, IHandshakeHandler
    {
        private bool m_Masked;

        private bool m_Final;

        private sbyte m_OpCode;

        private const byte FINAL_FLAG = 0x80;

        public DraftHybi10ReceiveFilter()
            : base(2)
        {

        }

        public void Handshake(IAppSession session, HttpHeaderInfo head)
        {
            throw new NotImplementedException();
        }

        private int GetPayloadLength(IList<ArraySegment<byte>> packageData, int length)
        {
            using (var reader = this.GetBufferReader(packageData))
            {
                if (length == 2)
                {
                    var flag = reader.ReadByte();

                    m_Final = (FINAL_FLAG & flag) == flag;
                    m_OpCode = (sbyte)(flag & 0x08);

                    // one byte playload length
                    var playloadLen = (int)reader.ReadByte();
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
                    reader.Skip(2);

                    // 2 bytes
                    var playloadLen = reader.ReadUInt16();

                    if (m_Masked) // add mask key's length
                        playloadLen += 4;

                    return playloadLen;
                }
                else // length = 8
                {
                    reader.Skip(2);

                    // 8 bytes
                    var playloadLen = reader.ReadUInt64();

                    if (m_Masked) // add mask key's length
                        playloadLen += 4;

                    return (int)playloadLen;
                }
            }
        }

        protected override int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length)
        {
            var session = AppContext.CurrentSession;
            var context = WebSocketContext.Get(session);

            var payloadLength = GetPayloadLength(packageData, length);

            if (payloadLength > 0)
                context.PayloadLength = payloadLength;

            context.OpCode = m_OpCode;

            return payloadLength;
        }

        public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var session = AppContext.CurrentSession;
            var context = WebSocketContext.Get(session);

            if (!m_Final)
            {
                context.AppendFragment(packageData);
                return null;
            }

            return context.ResolveLastFragment(packageData);
        }
    }
}
