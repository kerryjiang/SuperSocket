using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public class TerminatorCommandReader : CommandReaderBase<StringCommandInfo>
    {
        protected Encoding Encoding { get; private set; }
        private ICommandParser m_CommandParser;
        private SearchMarkState<byte> m_SearchState;

        public TerminatorCommandReader(IAppServer appServer)
            : base(appServer)
        {

        }

        public TerminatorCommandReader(CommandReaderBase<StringCommandInfo> previousCommandReader)
            : base(previousCommandReader)
        {

        }

        public TerminatorCommandReader(IAppServer appServer, Encoding encoding, byte[] terminator, ICommandParser commandParser)
            : this(appServer)
        {
            Encoding = encoding;
            m_SearchState = new SearchMarkState<byte>(terminator);
            m_CommandParser = commandParser;
        }

        public override StringCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left)
        {
            NextCommandReader = this;

            string command;

            if (!FindCommandInfoDirectly(readBuffer, offset, length, isReusableBuffer, out command, out left))
                return null;

            return m_CommandParser.ParseCommand(command);
        }        

        protected bool FindCommandInfoDirectly(byte[] readBuffer, int offset, int length, bool isReusableBuffer, out string command, out int left)
        {
            left = 0;

            int prevMatched = m_SearchState.Matched;

            int result = readBuffer.SearchMark(offset, length, m_SearchState);

            if (result < 0)
            {
                command = string.Empty;
                this.AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                return false;
            }

            int findLen = result - offset;

            if (this.BufferSegments.Count > 0)
            {
                if (findLen > 0)
                {
                    this.AddArraySegment(readBuffer, offset, findLen, false);
                    command = this.BufferSegments.Decode(Encoding);
                }
                else
                {
                    command = this.BufferSegments.Decode(Encoding, 0, this.BufferSegments.Count - prevMatched);
                }
            }
            else
            {
                command = Encoding.GetString(readBuffer, offset, findLen);
            }

            ClearBufferSegments();

            left = length - findLen - (m_SearchState.Mark.Length - prevMatched);
            return true;
        }
    }
}
