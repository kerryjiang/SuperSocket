using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The base class for command reader
    /// </summary>
    /// <typeparam name="TCommandInfo">The type of the command info.</typeparam>
    public abstract class BaseCommandReader<TCommandInfo> : ICommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        private List<ArraySegment<byte>> m_BufferSegments;

        /// <summary>
        /// Gets the buffer segments which can help you parse your commands conviniently.
        /// </summary>
        protected List<ArraySegment<byte>> BufferSegments
        {
            get
            {
                return m_BufferSegments;
            }
        }

        private BaseCommandReader()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandReaderBase&lt;TCommandInfo&gt;"/> class.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        public BaseCommandReader(IAppServer appServer)
        {
            AppServer = appServer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandReaderBase&lt;TCommandInfo&gt;"/> class.
        /// </summary>
        /// <param name="previousCommandReader">The previous command reader.</param>
        public BaseCommandReader(BaseCommandReader<TCommandInfo> previousCommandReader)
        {
            AppServer = previousCommandReader.AppServer;

            if (previousCommandReader.BufferSegments != null && previousCommandReader.BufferSegments.Count > 0)
                m_BufferSegments = previousCommandReader.BufferSegments;
        }

        #region ICommandReader<TCommandInfo> Members

        /// <summary>
        /// Gets the app server.
        /// </summary>
        public IAppServer AppServer { get; private set; }

        /// <summary>
        /// Finds the command info from current received read buffer.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the received data in readBuffer.</param>
        /// <param name="length">The length the received data.</param>
        /// <param name="isReusableBuffer">if set to <c>true</c> [is reusable buffer].</param>
        /// <param name="left">The size of left data which has not been parsed by this commandReader.</param>
        /// <returns>
        /// return the found commandInfo, return null if found nothing
        /// </returns>
        public abstract TCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get
            {
                return m_BufferSegments.GetTotalCount();
            }
        }

        /// <summary>
        /// Gets the command reader which will be used for next round receiving.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        public ICommandReader<TCommandInfo> NextCommandReader { get; protected set; }

        #endregion

        /// <summary>
        /// Adds the array into BufferSegments.
        /// </summary>
        /// <param name="buffer">The buffer which will be added into BufferSegments.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReusableBuffer">if set to <c>true</c> [is reusable buffer].</param>
        protected void AddArraySegment(byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            if (m_BufferSegments == null)
                m_BufferSegments = new List<ArraySegment<byte>>();

            if (isReusableBuffer)
                m_BufferSegments.Add(new ArraySegment<byte>(buffer.CloneRange(offset, length)));
            else
                m_BufferSegments.Add(new ArraySegment<byte>(buffer, offset, length));
        }

        /// <summary>
        /// Clears the buffer segments.
        /// </summary>
        protected void ClearBufferSegments()
        {
            if (m_BufferSegments != null)
                m_BufferSegments.Clear();
        }
    }
}
