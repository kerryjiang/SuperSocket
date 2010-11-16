using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public class AppendTerminatorCommandAsyncReader : TerminatorCommandAsyncReaderBase
    {
        private int m_PreviousMatchedCount = 0;
        private int m_CurrentMatchedCount = 0;

        public AppendTerminatorCommandAsyncReader(TerminatorCommandAsyncReaderBase prevCommandReader, int previousMatchedCount)
            : base(prevCommandReader)
        {
            m_PreviousMatchedCount = previousMatchedCount;
            m_CurrentMatchedCount = previousMatchedCount;
        }

        public override bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData)
        {
            commandData = new byte[0];
            //read count
            int i;

            for (i = 0; i < Math.Min(Terminator.Length - m_PreviousMatchedCount, length); i++)
            {
                if (readBuffer[offset + i] != Terminator[m_PreviousMatchedCount + i])
                {
                    m_CurrentMatchedCount = 0;
                    break;
                }

                m_CurrentMatchedCount++;
            }

            //skip checked bytes and then search endmark anew
            if (m_CurrentMatchedCount == 0)
            {
                var result = FindCommandDirectly(readBuffer, offset + i, length, out commandData);
                NextCommandReader = CreateNextCommandReader(result);
                return result.Status == SearhTokenStatus.Found;
            }

            //Found end
            if (m_CurrentMatchedCount == Terminator.Length)
            {
                SaveBuffer(readBuffer, offset, i);
                commandData = GetLeftBuffer().ToArrayData();
                ClearBuffer();
                SaveBuffer(readBuffer, offset + i, length - i);
                NextCommandReader = new NewTerminatorCommandAsyncReader(this);
                return true;
            }
            else
            {                
                SaveBuffer(readBuffer, offset, length);
                NextCommandReader = new AppendTerminatorCommandAsyncReader(this, m_CurrentMatchedCount);
                return false;
            }
        }
    }
}
