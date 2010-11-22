using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public enum SearhTokenStatus
    {
        None,
        Found,
        FoundStart
    }

    public class SearhTokenResult
    {
        public SearhTokenStatus Status { get; set; }
        public int Value { get; set; }
    }

    public class TerminatorCommandAsyncReader : ICommandAsyncReader<StringCommandInfo>
    {
        public byte[] Terminator { get; private set; }
        protected Encoding Encoding { get; private set; }
        private ICommandParser m_CommandParser;

        private ArraySegmentList<byte> m_BufferSegments;

        private TerminatorCommandAsyncReader()
        {
            m_BufferSegments = new ArraySegmentList<byte>();
        }

        public TerminatorCommandAsyncReader(Encoding encoding, string terminator, ICommandParser commandParser) : this()
        {
            Encoding = encoding;
            Terminator = encoding.GetBytes(terminator);
            m_CommandParser = commandParser;
        }

        #region IAsyncCommandReader Members

        public StringCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length)
        {
            NextCommandReader = this;

            string command;

            if (!FindCommandDirectly(readBuffer, offset, length, out command))
                return null;

            return m_CommandParser.ParseCommand(command);
        }

        public ICommandAsyncReader<StringCommandInfo> NextCommandReader { get; protected set; }

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return m_BufferSegments;
        }

        #endregion

        protected void ClearBuffer()
        {
            m_BufferSegments.ClearSegements();
        }

        protected bool FindCommandDirectly(byte[] readBuffer, int offset, int length, out string command)
        {
            var currentSegment = new ArraySegment<byte>(readBuffer, offset, length);
            m_BufferSegments.AddSegment(currentSegment);

            int? result = m_BufferSegments.SearchMark(Terminator);

            if (!result.HasValue)
            {
                command = string.Empty;
                return false;
            }

            if (result.Value < 0)
            {
                command = string.Empty;
                return false;
            }

            int findLen = result.Value + Terminator.Length;
            int total = m_BufferSegments.Count;
            command = Encoding.GetString(m_BufferSegments.ToArrayData(0, result.Value));
            ClearBuffer();

            if (findLen < total)
            {
                int left = total - findLen;
                m_BufferSegments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));
            }

            return true;
        }
    }
}
