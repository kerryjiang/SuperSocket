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

        internal void Initialize(string commandName, int length, CommandReaderBase<BinaryCommandInfo> previousCommandReader)
        {
            m_Length = length;
            m_CommandName = commandName;

            base.Initialize(previousCommandReader);
        }

        public override BinaryCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            left = 0;

            int leftLength = m_Length - BufferSegments.Count;

            AddArraySegment(readBuffer, offset, length, isReusableBuffer);

            if (length >= leftLength)
            {
                NextCommandReader = new MyCommandReader(AppServer);
                var commandInfo = new BinaryCommandInfo(m_CommandName, BufferSegments.ToArrayData(0, m_Length));

                if (length > leftLength)
                    left = length - leftLength;

                return commandInfo;
            }
            else
            {
                NextCommandReader = this;
                return null;
            }
        }
    }
}
