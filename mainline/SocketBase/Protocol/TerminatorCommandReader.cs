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

    public class TerminatorCommandReader : CommandReaderBase<StringCommandInfo>
    {
        public byte[] Terminator { get; private set; }
        protected Encoding Encoding { get; private set; }
        private ICommandParser m_CommandParser;

        public TerminatorCommandReader(IAppServer appServer)
            : base(appServer)
        {

        }

        public TerminatorCommandReader(ICommandReader<StringCommandInfo> previousCommandReader)
            : base(previousCommandReader)
        {

        }

        public TerminatorCommandReader(IAppServer appServer, Encoding encoding, string terminator, ICommandParser commandParser)
            : this(appServer)
        {
            Encoding = encoding;
            Terminator = encoding.GetBytes(terminator);
            m_CommandParser = commandParser;
        }

        public override StringCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer)
        {
            NextCommandReader = this;

            string command;

            if (!FindCommandDirectly(readBuffer, offset, length, isReusableBuffer, out command))
                return null;

            return m_CommandParser.ParseCommand(command);
        }

        protected void ClearBuffer()
        {
            BufferSegments.ClearSegements();
        }

        protected bool FindCommandDirectly(byte[] readBuffer, int offset, int length, bool isReusableBuffer, out string command)
        {
            ArraySegment<byte> currentSegment;

            if (isReusableBuffer)
            {
                //Next received data also will be saved in this buffer, so we should create a new byte[] to persistent current received data
                currentSegment = new ArraySegment<byte>(readBuffer.CloneRange(offset, length));
            }
            else
            {
                currentSegment = new ArraySegment<byte>(readBuffer, offset, length);
            }

            BufferSegments.AddSegment(currentSegment);

            int? result = BufferSegments.SearchMark(Terminator);

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
            int total = BufferSegments.Count;
            command = Encoding.GetString(BufferSegments.ToArrayData(0, result.Value));
            ClearBuffer();

            if (findLen < total)
            {
                int left = total - findLen;

                if (isReusableBuffer)
                    BufferSegments.AddSegment(new ArraySegment<byte>(readBuffer.CloneRange(offset + length - left, left)));
                else
                    BufferSegments.AddSegment(new ArraySegment<byte>(readBuffer, offset + length - left, left));
            }

            return true;
        }
    }
}
