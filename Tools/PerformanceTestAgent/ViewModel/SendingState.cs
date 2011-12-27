using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PerformanceTestAgent.ViewModel
{
    class SendingState
    {
        public SendingState(Socket socket, string content, Encoding encoding, int expectedReceiveLength)
        {
            Content = content.ToCharArray();
            m_Encoder = encoding.GetEncoder();
            Socket = socket;
            ExpectedReceiveLength = expectedReceiveLength;
        }

        public char[] Content { get; private set; }

        public int ConvertedChars { get; private set; }

        public Socket Socket { get; private set; }

        public int TotalBytes { get; private set; }

        public int ExpectedReceiveLength { get; private set; }

        public bool IsCompleted
        {
            get
            {
                return ConvertedChars >= Content.Length;
            }
        }

        private Encoder m_Encoder;

        public int Encode(byte[] buffer, int offset, int length)
        {
            int charsUsed, bytesUsed;
            bool completed;

            m_Encoder.Convert(Content, ConvertedChars, Content.Length - ConvertedChars, buffer, offset, length, true, out charsUsed, out bytesUsed, out completed);

            ConvertedChars += charsUsed;
            TotalBytes += bytesUsed;

            return bytesUsed;
        }

        public void Clear()
        {
            m_Encoder = null;
            Socket = null;
            Content = null;
        }
    }
}
