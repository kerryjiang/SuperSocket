using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public class TerminatorCommandStreamReader : ICommandStreamReader
    {
        private char[] m_TerminatorChars;
        private ArraySegmentList<char> m_BufferSegments;
        private Encoding m_CommandEncoding;
        private Stream m_UnderlyingStream;
        private int m_BufferSize;
        private byte[] m_Buffer;
        private int m_BufferOffset;

        public TerminatorCommandStreamReader(string terminator)
        {
            m_BufferSegments = new ArraySegmentList<char>();
            m_TerminatorChars = terminator.ToArray();
        }

        #region ICommandStreamReader Members

        public void InitializeReader(Stream stream, Encoding encoding, int bufferSize)
        {
            m_UnderlyingStream = stream;
            m_CommandEncoding = encoding;
            m_BufferSize = bufferSize;
            m_Buffer = new byte[bufferSize];
        }

        public string ReadCommand()
        {
            var decoder = m_CommandEncoding.GetDecoder();

            while (true)
            {
                int thisRead = m_UnderlyingStream.Read(m_Buffer, m_BufferOffset, m_Buffer.Length);
                int maxCharCount = m_CommandEncoding.GetMaxCharCount(thisRead + m_BufferOffset);
                var charsBuffer = new char[maxCharCount];
                int bytesUsed, charsUsed;
                bool completed = false;

                decoder.Convert(m_Buffer, 0, thisRead + m_BufferOffset, charsBuffer, 0, maxCharCount, false,
                    out bytesUsed, out charsUsed, out completed);

                if (!completed)
                {
                    Buffer.BlockCopy(m_Buffer, bytesUsed, m_Buffer, 0, thisRead + m_BufferOffset - bytesUsed);
                    m_BufferOffset = bytesUsed;
                }
                else
                {
                    m_BufferOffset = 0;
                }
        
                m_BufferSegments.AddSegment(new ArraySegment<char>(charsBuffer, 0, charsUsed));
                var result = m_BufferSegments.SearchMark(m_TerminatorChars);

                if (result.HasValue && result.Value > 0)
                {
                    string commandToken = new string(m_BufferSegments.ToArrayData(0, result.Value));

                    int left = m_BufferSegments.Count - result.Value - m_TerminatorChars.Length;
                    m_BufferSegments.ClearSegements();
                    if (left > 0)
                        m_BufferSegments.AddSegment(new ArraySegment<char>(charsBuffer, charsUsed - left, left));

                    return commandToken;
                }
            }
        }

        #endregion
    }
}
