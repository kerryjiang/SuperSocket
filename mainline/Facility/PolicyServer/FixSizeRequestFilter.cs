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
    /// <summary>
    /// FixSizeRequestFilter
    /// </summary>
    public class FixSizeRequestFilter : IRequestFilter<BinaryRequestInfo>
    {
        private byte[] m_Buffer;
        private int m_CurrentReceived = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixSizeRequestFilter"/> class.
        /// </summary>
        /// <param name="fixCommandSize">Size of the fix command.</param>
        public FixSizeRequestFilter(int fixCommandSize)
        {
            m_Buffer = new byte[fixCommandSize];
        }

        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left, the length of the data which hasn't been parsed.</param>
        /// <returns>return the parsed TRequestInfo</returns>
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

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_CurrentReceived; }
        }

        /// <summary>
        /// Gets the next request filter.
        /// </summary>
        public IRequestFilter<BinaryRequestInfo> NextRequestFilter
        {
            get { return null; }
        }
    }
}
