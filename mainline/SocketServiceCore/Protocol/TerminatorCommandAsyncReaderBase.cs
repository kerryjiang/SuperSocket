using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.AsyncSocket;

namespace SuperSocket.SocketServiceCore.Protocol
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

    public abstract class TerminatorCommandAsyncReaderBase : ICommandAsyncReader
    {
        public byte[] Terminator { get; private set; }

        private ArraySegmentList<byte> m_BufferSegments;

        private TerminatorCommandAsyncReaderBase()
        {
            m_BufferSegments = new ArraySegmentList<byte>();
        }

        public TerminatorCommandAsyncReaderBase(byte[] token) : this()
        {
            Terminator = token;            
        }

        public TerminatorCommandAsyncReaderBase(TerminatorCommandAsyncReaderBase prevCommandAsyncReader)
        {
            m_BufferSegments = prevCommandAsyncReader.GetLeftBuffer();
            Terminator = prevCommandAsyncReader.Terminator;
        }

        #region IAsyncCommandReader Members

        public abstract bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData);

        public ArraySegmentList<byte> GetLeftBuffer()
        {
            return m_BufferSegments;
        }

        public ICommandAsyncReader NextCommandReader { get; protected set; }

        #endregion

        protected void SaveBuffer(byte[] buffer, int offset, int length)
        {
            byte[] segment = new byte[length];
            Buffer.BlockCopy(buffer, offset, segment, 0, length);
            m_BufferSegments.AddSegment(new ArraySegment<byte>(segment, 0, length));
        }

        protected void ClearBuffer()
        {
            m_BufferSegments.ClearSegements();
        }

        protected SearhTokenResult FindCommandDirectly(byte[] readBuffer, int offset, int length, out byte[] commandData)
        {
            commandData = new byte[0];

            int? result = readBuffer.SearchMark(offset, length, Terminator);

            if (!result.HasValue)
            {
                SaveBuffer(readBuffer, offset, length);
                commandData = new byte[0];
                return new SearhTokenResult
                {
                    Status = SearhTokenStatus.None
                };
            }

            //Found endmark
            if (result.Value > 0)
            {
                SaveBuffer(readBuffer, offset, result.Value - offset);
                commandData = m_BufferSegments.ToArrayData();
                ClearBuffer();

                if(offset + length - 1 > result.Value + Terminator.Length)
                    SaveBuffer(readBuffer, result.Value + Terminator.Length, length - result.Value - Terminator.Length);

                return new SearhTokenResult
                {
                    Status = SearhTokenStatus.Found,
                    Value = result.Value
                };
            }
            else
            {
                SaveBuffer(readBuffer, offset, length);
                return new SearhTokenResult
                {
                    Status = SearhTokenStatus.FoundStart,
                    Value = 0 - result.Value
                };
            }
        }

        protected ICommandAsyncReader CreateNextCommandReader(SearhTokenResult result)
        {
            if (result.Status == SearhTokenStatus.FoundStart)
            {
                return new AppendTerminatorCommandAsyncReader(this, 0 - result.Value);
            }
            else if (result.Status == SearhTokenStatus.Found)
            {
                return new NewTerminatorCommandAsyncReader(this);
            }
            else
            {
                return new NewTerminatorCommandAsyncReader(this);
            }
        }
    }
}
