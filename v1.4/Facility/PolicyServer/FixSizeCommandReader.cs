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
    public class FixSizeCommandReader : ICommandReader<BinaryCommandInfo>
    {
        private byte[] m_Buffer;
        private int m_CurrentReceived = 0;

        public FixSizeCommandReader(IAppServer appServer, int fixCommandSize)
        {
            AppServer = appServer;
            m_Buffer = new byte[fixCommandSize];
        }

        public IAppServer AppServer { get; private set; }

        public BinaryCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
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

            return new BinaryCommandInfo("REQU", m_Buffer);
        }

        public byte[] GetLeftBuffer()
        {
            return m_Buffer.Take(m_CurrentReceived).ToArray();
        }

        public int LeftBufferSize
        {
            get { return m_CurrentReceived; }
        }

        public ICommandReader<BinaryCommandInfo> NextCommandReader
        {
            get { return null; }
        }
    }
}
