using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.SocksServer
{
    public class ConnectCommandReader : CommandReaderBase<BinaryCommandInfo>
    {
        private int m_SocksVersion = -1;
        private bool m_StartDetectEndMark = false;
        private const byte m_EndMark = 0x00;

        public ConnectCommandReader(IAppServer appServer)
            : base(appServer)
        {

        }

        public override BinaryCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            //First data
            if (m_SocksVersion < 0)
            {
                m_SocksVersion = (int)readBuffer[offset];
                if (m_SocksVersion != 4)
                    return null;

                ((SocksSocketContext)context).SocksVersion = m_SocksVersion;
 
                return ProcessDataNotStartDetectMark(readBuffer, offset, length, isReusableBuffer);
            }
            else
            {
                if (!m_StartDetectEndMark)
                    return ProcessDataNotStartDetectMark(readBuffer, offset, length, isReusableBuffer);
                else
                    return ProcessDataStartDetectMark(readBuffer, offset, length, isReusableBuffer);
            }
        }

        private BinaryCommandInfo ProcessDataStartDetectMark(byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            int endPos = readBuffer.IndexOf(m_EndMark, offset, length);
            if (endPos >= 0)
            {
                int lastPos = offset + length - 1;

                byte[] data;
                if (BufferSegments.Count > 0)
                {
                    this.AddArraySegment(readBuffer, offset, endPos - offset + 1, isReusableBuffer);
                    data = this.BufferSegments.ToArrayData();
                }
                else
                {
                    data = readBuffer.CloneRange(offset, endPos - offset + 1);
                }

                BufferSegments.ClearSegements();

                if (endPos < lastPos)//has left data
                    this.AddArraySegment(readBuffer, endPos + 1, length - (endPos - offset + 1), isReusableBuffer);

                this.NextCommandReader = new DataCommandReader(this);
                return new BinaryCommandInfo(SocksConst.CONN, data);
            }
            else
            {
                this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                this.NextCommandReader = this;
                return null;
            }
        }

        private BinaryCommandInfo ProcessDataNotStartDetectMark(byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            int left = 8 - this.BufferSegments.Count;

            if (length < left)
            {
                this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                this.NextCommandReader = this;
                return null;
            }
            else if (length == left)
            {
                m_StartDetectEndMark = true;
                this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                this.NextCommandReader = this;
                return null;
            }
            else
            {
                m_StartDetectEndMark = true;
                return ProcessDataStartDetectMark(readBuffer, offset, length, isReusableBuffer);
            }
        }
    }
}
