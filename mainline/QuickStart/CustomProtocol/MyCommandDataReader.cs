using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyCommandDataReader : CommandReaderBase<BinaryCommandInfo>
    {
        private int m_Length;

        private string m_CommandName;

        public MyCommandDataReader(string commandName, int length, CommandReaderBase<BinaryCommandInfo> previousCommandReader)
            : base(previousCommandReader)
        {
            m_Length = length;
            m_CommandName = commandName;
        }

        public override BinaryCommandInfo FindCommandInfo(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            int leftLength = m_Length - BufferSegments.Count;

            if (length <= leftLength)
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);

                if (length == leftLength)
                {
                    NextCommandReader = new MyCommandReader(AppServer);
                    return new BinaryCommandInfo(m_CommandName, BufferSegments.ToArrayData());
                }
                else
                {
                    NextCommandReader = this;
                    return null;
                }
            }
            else
            {
                throw new Exception("Unordered message!");
            }
        }
    }
}
