using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
{
    public class SwitchReceiveFilter : IReceiveFilter<StringRequestInfo>
    {
        private IReceiveFilter<StringRequestInfo> m_FilterA;
        private byte m_BeginMarkA = (byte)'Y';

        private IReceiveFilter<StringRequestInfo> m_FilterB;
        private byte m_BeginMarkB = (byte)'*';

        public SwitchReceiveFilter()
        {
            m_FilterA = new ReceiveFilterA(this);
            m_FilterB = new ReceiveFilterB(this);
        }

        public StringRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = length;
            var flag = readBuffer[offset];

            if (flag == m_BeginMarkA)
                NextReceiveFilter = m_FilterA;
            else if (flag == m_BeginMarkB)
                NextReceiveFilter = m_FilterB;
            else
                State = FilterState.Error;

            return null;
        }

        public StringRequestInfo Filter(ReceiveCache data, out int rest)
        {
            var current = data.Current;
            rest = current.Count;

            var flag = current.Array[current.Offset];

            if (flag == m_BeginMarkA)
                NextReceiveFilter = m_FilterA;
            else if (flag == m_BeginMarkB)
                NextReceiveFilter = m_FilterB;
            else
                State = FilterState.Error;

            return null;
        }

        public IReceiveFilter<StringRequestInfo> NextReceiveFilter { get; private set; }

        public void Reset()
        {

        }

        public FilterState State { get; private set; }
    }
}
