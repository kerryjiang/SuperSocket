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

        public CommandReaderBase(ICommandReader<TCommandInfo> previousCommandReader)
        {
            AppServer = previousCommandReader.AppServer;
            m_BufferSegments = previousCommandReader.GetLeftBuffer();
        }

        #region ICommandReader<TCommandInfo> Members

        public IAppServer AppServer { get; private set; }

        public TCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            var commandInfo = FindCommandInfo(context, readBuffer, offset, length, isReusableBuffer);

            if (commandInfo == null)
            {
                if (m_BufferSegments.Count >= AppServer.Config.MaxCommandLength)
                    throw new ExceedMaxCommandLengthException(AppServer.Config.MaxCommandLength, m_BufferSegments.Count);
            }

            return commandInfo;
        }

        protected abstract TCommandInfo FindCommandInfo(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return m_BufferSegments;
        }

        public ICommandReader<TCommandInfo> NextCommandReader { get; protected set; }

        #endregion
    }
}
