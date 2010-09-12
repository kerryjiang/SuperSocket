using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    class PartMatchedAsyncCommandReader : AsyncCommandReader
    {
        private int m_PreviusMatchedCount = 0;
        private int m_CurrentMatchedCount = 0;

        public PartMatchedAsyncCommandReader(IAsyncCommandReader prevReader, int previusMatchedCount)
            : base(prevReader)
        {
            m_PreviusMatchedCount = previusMatchedCount;
            m_CurrentMatchedCount = previusMatchedCount;
        }

        public override SearhMarkResult FindCommand(SocketAsyncEventArgs e, byte[] endMark, out byte[] commandData)
        {
            //read count
            int i;

            for (i = 0; i < Math.Min(endMark.Length - m_PreviusMatchedCount, e.BytesTransferred); i++)
            {
                if (e.Buffer[e.Offset + i] != endMark[m_PreviusMatchedCount + i])
                {
                    m_CurrentMatchedCount = 0;
                    break;
                }

                m_CurrentMatchedCount++;
            }

            //skip checked bytes and then search endmark anew
            if (m_CurrentMatchedCount == 0)                
                return FindCommandDirectly(e, e.Offset + i, endMark, out commandData);

            //Found end
            if (m_CurrentMatchedCount == endMark.Length)
            {
                var buffer = SaveBuffer(e.Buffer, e.Offset, i);
                commandData = buffer.Take(buffer.Count - endMark.Length).ToArray();
                buffer.Clear();
                SaveBuffer(e.Buffer, e.Offset + i, e.BytesTransferred - i);
                return new SearhMarkResult
                {
                    Status = SearhMarkStatus.Found,
                    Value = e.Offset
                };
            }
            else
            {
                commandData = new byte[0];
                SaveBuffer(e.Buffer, e.Offset, e.BytesTransferred);
                return new SearhMarkResult
                {
                    Status = SearhMarkStatus.FoundStart,
                    Value = m_CurrentMatchedCount
                };
            }
        }
    }
}
