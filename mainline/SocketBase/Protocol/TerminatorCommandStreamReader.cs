using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public class TerminatorCommandStreamReader : ICommandStreamReader<StringCommandInfo>
    {
        private char[] m_TerminatorChars;
        private ArraySegmentList<char> m_BufferSegments;
        private Encoding m_CommandEncoding;
        private Decoder m_Decoder;
        private Stream m_UnderlyingStream;
        private int m_BufferSize;
        private byte[] m_Buffer;
        private ICommandParser m_CommandParser;

        public TerminatorCommandStreamReader(string terminator, ICommandParser commandParser)
        {
            m_BufferSegments = new ArraySegmentList<char>();
            m_TerminatorChars = terminator.ToArray();
            m_CommandParser = commandParser;
        }

        #region ICommandStreamReader Members

        public void InitializeReader(Stream stream, Encoding encoding, int bufferSize)
        {
            m_UnderlyingStream = stream;
            m_CommandEncoding = encoding;
            m_BufferSize = bufferSize;
            m_Buffer = new byte[bufferSize];
            m_Decoder = m_CommandEncoding.GetDecoder();
        }

        public StringCommandInfo ReadCommand()
        {
            while (true)
            {
                int thisRead = m_UnderlyingStream.Read(m_Buffer, 0, m_Buffer.Length);
                int maxCharCount = m_CommandEncoding.GetMaxCharCount(thisRead);
                var charsBuffer = new char[maxCharCount];
                int bytesUsed, charsUsed;
                bool completed = false;

                m_Decoder.Convert(m_Buffer, 0, thisRead, charsBuffer, 0, maxCharCount, false,
                    out bytesUsed, out charsUsed, out completed);
        
                m_BufferSegments.AddSegment(new ArraySegment<char>(charsBuffer, 0, charsUsed));
                var result = m_BufferSegments.SearchMark(m_TerminatorChars);

                if (result.HasValue && result.Value > 0)
                {
                    string commandToken = new string(m_BufferSegments.ToArrayData(0, result.Value));

                    int left = m_BufferSegments.Count - result.Value - m_TerminatorChars.Length;
                    m_BufferSegments.ClearSegements();
                    if (left > 0)
                        m_BufferSegments.AddSegment(new ArraySegment<char>(charsBuffer, charsUsed - left, left));

                    return m_CommandParser.ParseCommand(commandToken);
                }
            }
        }

        #endregion
    }
}
