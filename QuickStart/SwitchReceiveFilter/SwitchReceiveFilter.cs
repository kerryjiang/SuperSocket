using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
{
    public class SwitchReceiveFilter : IReceiveFilter<StringPackageInfo>
    {
        private IReceiveFilter<StringPackageInfo> m_FilterA;
        private byte m_BeginMarkA = (byte)'Y';

        private IReceiveFilter<StringPackageInfo> m_FilterB;
        private byte m_BeginMarkB = (byte)'*';

        public SwitchReceiveFilter()
        {
            m_FilterA = new ReceiveFilterA(this);
            m_FilterB = new ReceiveFilterB(this);
        }

        public StringPackageInfo Filter(BufferList data, out int rest)
        {
            var current = data.Last;
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

        public IReceiveFilter<StringPackageInfo> NextReceiveFilter { get; private set; }

        public void Reset()
        {

        }

        public FilterState State { get; private set; }
    }
}
