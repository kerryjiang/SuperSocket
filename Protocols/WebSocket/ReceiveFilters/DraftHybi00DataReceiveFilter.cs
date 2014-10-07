using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class DraftHybi00DataReceiveFilter : IReceiveFilter<StringPackageInfo>
    {
        class MarkReceiveFilter : TerminatorReceiveFilter<StringPackageInfo>
        {
            private static byte[] m_EndMark = new byte[] { 0xff };

            private IReceiveFilter<StringPackageInfo> m_SwitchReceiveFilter;

            public MarkReceiveFilter(IReceiveFilter<StringPackageInfo> switchReceiveFilter)
                : base(m_EndMark)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var session = AppContext.CurrentSession;
                var context = WebSocketContext.Get(session);
                context.OpCode = OpCode.Text;
                context.PayloadLength = packageData.Sum(d => d.Count);
                NextReceiveFilter = m_SwitchReceiveFilter;
                return context.ResolveLastFragment(packageData);
            }
        }

        class CloseMarkReceiveFilter : TerminatorReceiveFilter<StringPackageInfo>
        {
            IReceiveFilter<StringPackageInfo> m_SwitchReceiveFilter;
            private static byte[] m_EndMark = new byte[] { 0x00 };

            public CloseMarkReceiveFilter(IReceiveFilter<StringPackageInfo> switchReceiveFilter)
                : base(m_EndMark)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                return null;
            }
        }

        class LenReceiveFilter : FixedHeaderReceiveFilter<StringPackageInfo>
        {
            private IReceiveFilter<StringPackageInfo> m_SwitchReceiveFilter;

            public LenReceiveFilter(IReceiveFilter<StringPackageInfo> switchReceiveFilter)
                : base(3)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            protected override int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length)
            {
                throw new NotImplementedException();
            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                NextReceiveFilter = m_SwitchReceiveFilter;
                return null;
            }
        }

        private IReceiveFilter<StringPackageInfo> m_MarkReceiveFilter;
        private IReceiveFilter<StringPackageInfo> m_LenReceiveFilter;
        private IReceiveFilter<StringPackageInfo> m_CloseMarkReceiveFilter;

        public DraftHybi00DataReceiveFilter()
        {
            m_MarkReceiveFilter = new MarkReceiveFilter(this);
            m_LenReceiveFilter = new LenReceiveFilter(this);
            m_CloseMarkReceiveFilter = new CloseMarkReceiveFilter(this);
        }

        public StringPackageInfo Filter(BufferList data, out int rest)
        {
            rest = data.Total;
            var current = data.Last;
            var startByte = current.Array[current.Offset];

            if ((startByte & 0x80) == 0x00) //data is fragmented by begin/end mark
            {
                rest--;
                NextReceiveFilter = m_MarkReceiveFilter;
            }
            else if (startByte == 0xff)
            {
                rest--;
                NextReceiveFilter = m_CloseMarkReceiveFilter;
            }
            else //data is fragmented by length
            {
                NextReceiveFilter = m_LenReceiveFilter;
            }

            return null;
        }

        public IReceiveFilter<StringPackageInfo> NextReceiveFilter { get; private set; }

        public FilterState State { get; private set; }

        public void Reset()
        {
            
        }
    }
}
