using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    /// <summary>
    /// It is the kind of protocol that
    /// the first two bytes of each command are { 0x68, 0x68 }
    /// and the last two bytes of each command are { 0x0d, 0x0a }
    /// and the 16th byte (data[15]) of each command indicate the command type
    /// if data[15] = 0x10, the command is a keep alive one
    /// if data[15] = 0x1a, the command is position one
    /// </summary>
    class GPSCommandReader : CommandReaderBase<BinaryCommandInfo>
    {
        private static byte[] m_StartMark = new byte[] { 0x68, 0x68 };
        private static byte[] m_EndMark = new byte[] { 0x0d, 0x0a };

        private bool m_FoundStart = false;
        private int m_StartPos = -1;

        public GPSCommandReader(IAppServer appServer)
            : base(appServer)
        {
            
        }

        public override BinaryCommandInfo FindCommandInfo(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            left = 0;

            int lastSegmentOffset = BufferSegments.Count;
            int searchEndMarkLength = length;

            this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            if (!m_FoundStart)
            {
                var pos = BufferSegments.SearchMark(lastSegmentOffset, length, m_StartMark);
                
                if (!pos.HasValue || pos.Value < 0)
                    return null;

                //Found start mark
                m_StartPos = pos.Value;
                m_FoundStart = true;

                lastSegmentOffset = pos.Value + 2;

                //The end mark could not exist in this round received data
                if (lastSegmentOffset + 2 > BufferSegments.Count)
                    return null;

                searchEndMarkLength = BufferSegments.Count - lastSegmentOffset;
            }

            var endPos = BufferSegments.SearchMark(lastSegmentOffset, searchEndMarkLength, m_EndMark);
            //Haven't found end mark
            if (!endPos.HasValue || endPos.Value < 0)
                return null;

            //Found end mark
            left = BufferSegments.Count - endPos.Value - 2;

            var commandData = BufferSegments.ToArrayData(m_StartPos, endPos.Value - m_StartPos + 2);
            var commandInfo = new BinaryCommandInfo(BitConverter.ToString(commandData, 15, 1), commandData);

            //Reset state
            m_FoundStart = false;
            m_StartPos = -1;

            BufferSegments.ClearSegements();

            return commandInfo;
        }
    }
}
