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
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class CommandReaderBase<TRequestInfo> : ICommandReader<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private ArraySegmentList m_BufferSegments;

        /// <summary>
        /// Gets the buffer segments which can help you parse your commands conviniently.
        /// </summary>
        protected ArraySegmentList BufferSegments
        {
            get { return m_BufferSegments; }
        }

        public CommandReaderBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandReaderBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        public CommandReaderBase(IAppServer appServer)
        {
            AppServer = appServer;
            m_BufferSegments = new ArraySegmentList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandReaderBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="previousCommandReader">The previous command reader.</param>
        public CommandReaderBase(CommandReaderBase<TRequestInfo> previousCommandReader)
        {
            AppServer = previousCommandReader.AppServer;
            m_BufferSegments = previousCommandReader.BufferSegments;
        }

        /// <summary>
        /// Initializes the instance with the specified previous command reader.
        /// </summary>
        /// <param name="previousCommandReader">The previous command reader.</param>
        protected void Initialize(CommandReaderBase<TRequestInfo> previousCommandReader)
        {
            AppServer = previousCommandReader.AppServer;
            m_BufferSegments = previousCommandReader.BufferSegments;
        }

        #region ICommandReader<TRequestInfo> Members

        /// <summary>
        /// Gets the app server.
        /// </summary>
        public IAppServer AppServer { get; private set; }

        /// <summary>
        /// Finds the request info from current received read buffer.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the received data in readBuffer.</param>
        /// <param name="length">The length the received data.</param>
        /// <param name="isReusableBuffer">if set to <c>true</c> [is reusable buffer].</param>
        /// <param name="left">The size of left data which has not been parsed by this commandReader.</param>
        /// <returns>
        /// return the found requestInfo, return null if found nothing
        /// </returns>
        public abstract TRequestInfo FindRequestInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left);

        /// <summary>
        /// Gets the left buffer.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        protected byte[] GetLeftBuffer()
        {
            return m_BufferSegments.ToArrayData();
        }

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        public int LeftBufferSize
        {
            get { return m_BufferSegments.Count; }
        }

        /// <summary>
        /// Gets the command reader which will be used for next round receiving.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        public ICommandReader<TRequestInfo> NextCommandReader { get; protected set; }

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
            m_BufferSegments.AddSegment(buffer, offset, length, isReusableBuffer);
        }

        /// <summary>
        /// Clears the buffer segments.
        /// </summary>
        protected void ClearBufferSegments()
        {
            m_BufferSegments.ClearSegements();
        }
    }
}
