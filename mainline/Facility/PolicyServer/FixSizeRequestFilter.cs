using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.Common;

namespace SuperSocket.Facility.PolicyServer
{
    public class FixSizeRequestFilter : IRequestFilter<BinaryRequestInfo>
    {
        private byte[] m_Buffer;
        private int m_CurrentReceived = 0;

        public FixSizeRequestFilter(int fixCommandSize)
        {
            m_Buffer = new byte[fixCommandSize];
        }

        public BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

            if (m_CurrentReceived + length <= m_Buffer.Length)
            {
                Array.Copy(readBuffer, offset, m_Buffer, m_CurrentReceived, length);
                m_CurrentReceived += length;

                if (m_CurrentReceived < m_Buffer.Length)
                    return null;
            }
            else
            {
                Array.Copy(readBuffer, offset, m_Buffer, m_CurrentReceived, m_Buffer.Length - m_CurrentReceived);
            }

            m_CurrentReceived = 0;

            return new BinaryRequestInfo("REQU", m_Buffer);
        }

        public int LeftBufferSize
        {
            get { return m_CurrentReceived; }
        }

        public IRequestFilter<BinaryRequestInfo> NextRequestFilter
        {
            get { return null; }
        }
    }
}
