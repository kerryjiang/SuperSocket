using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    public abstract class CommandReaderBase<TCommandInfo> : ICommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        private readonly ArraySegmentList<byte> m_BufferSegments;

        protected ArraySegmentList<byte> BufferSegments
        {
            get { return m_BufferSegments; }
        }

        private CommandReaderBase()
        {

        }

        public CommandReaderBase(IAppServer appServer)
        {
            AppServer = appServer;
            m_BufferSegments = new ArraySegmentList<byte>();
        }

        public CommandReaderBase(CommandReaderBase<TCommandInfo> previousCommandReader)
        {
            AppServer = previousCommandReader.AppServer;
            m_BufferSegments = previousCommandReader.BufferSegments;
        }

        #region ICommandReader<TCommandInfo> Members

        public IAppServer AppServer { get; private set; }

        public virtual TCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            left = 0;
            return FindCommand(context, readBuffer, offset, length, isReusableBuffer);
        }

        public virtual TCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            return default(TCommandInfo);
        }

        public byte[] GetLeftBuffer()
        {
            return m_BufferSegments.ToArrayData();
        }

        public int LeftBufferSize
        {
            get { return m_BufferSegments.Count; }
        }

        public ICommandReader<TCommandInfo> NextCommandReader { get; protected set; }

        #endregion

        protected void AddArraySegment(byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            if (isReusableBuffer)
                BufferSegments.AddSegment(new ArraySegment<byte>(buffer.CloneRange(offset, length)));
            else
                BufferSegments.AddSegment(new ArraySegment<byte>(buffer, offset, length));
        }
    }
}
