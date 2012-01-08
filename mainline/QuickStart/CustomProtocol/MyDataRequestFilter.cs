using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomProtocol
{
    class MyDataRequestFilter : RequestFilterBase<BinaryRequestInfo>
    {
        private int m_Length;

        private string m_CommandName;

        internal void Initialize(string commandName, int length, RequestFilterBase<BinaryRequestInfo> previousCommandReader)
        {
            m_Length = length;
            m_CommandName = commandName;

            base.Initialize(previousCommandReader);
        }

        public override BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

            int leftLength = m_Length - BufferSegments.Count;

            AddArraySegment(readBuffer, offset, length, toBeCopied);

            if (length >= leftLength)
            {
                NextRequestFilter = new MyRequestFilter();
                var requestInfo = new BinaryRequestInfo(m_CommandName, BufferSegments.ToArrayData(0, m_Length));

                if (length > leftLength)
                    left = length - leftLength;

                return requestInfo;
            }
            else
            {
                NextRequestFilter = this;
                return null;
            }
        }
    }
}
