using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class DraftHybi00DataReceiveFilter : IWebSocketReceiveFilter, IReceiveFilter<WebSocketPackageInfo>
    {
        class MarkReceiveFilter : TerminatorReceiveFilter<WebSocketPackageInfo>
        {
            private static byte[] m_EndMark = new byte[] { 0xff };

            private IWebSocketReceiveFilter m_SwitchReceiveFilter;

            public MarkReceiveFilter(IWebSocketReceiveFilter switchReceiveFilter)
                : base(m_EndMark)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            public override WebSocketPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var context = m_SwitchReceiveFilter.Context;
                context.OpCode = OpCode.Text;
                context.PayloadLength = packageData.Sum(d => d.Count);
                NextReceiveFilter = m_SwitchReceiveFilter;
                return context.ResolveLastFragment(packageData);
            }
        }

        class CloseMarkReceiveFilter : TerminatorReceiveFilter<WebSocketPackageInfo>
        {
            IWebSocketReceiveFilter m_SwitchReceiveFilter;
            private static byte[] m_EndMark = new byte[] { 0x00 };

            public CloseMarkReceiveFilter(IWebSocketReceiveFilter switchReceiveFilter)
                : base(m_EndMark)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            public override WebSocketPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                var context = m_SwitchReceiveFilter.Context;
                context.OpCode = OpCode.Close;
                context.PayloadLength = packageData.Sum(d => d.Count);
                NextReceiveFilter = m_SwitchReceiveFilter;
                return context.ResolveLastFragment(packageData);
            }
        }

        class LenReceiveFilter : IReceiveFilter<WebSocketPackageInfo>
        {
            private IWebSocketReceiveFilter m_SwitchReceiveFilter;
            private int m_Length = 0;
            private bool m_LengthLoaded = false;

            public LenReceiveFilter(IWebSocketReceiveFilter switchReceiveFilter)
            {
                m_SwitchReceiveFilter = switchReceiveFilter;
            }

            public IReceiveFilter<WebSocketPackageInfo> NextReceiveFilter { get; private set; }

            public FilterState State { get; private set; }

            public WebSocketPackageInfo Filter(BufferList data, out int rest)
            {
                rest = 0;

                if (!m_LengthLoaded)
                {
                    var current = data.Last;

                    var tempLen = m_Length;

                    for(var i = 0; i < current.Count; i++)
                    {
                        var len = (int)current.Array[current.Offset + i];

                        if (len >= 128)
                        {
                            tempLen = tempLen * 128 + len - 128;
                            m_Length = data.Total - current.Count + i + 1 + tempLen; // total length
                            m_LengthLoaded = true;
                            break;
                        }

                        tempLen = tempLen * 128 + len;
                        m_Length = tempLen;
                    }

                    if (!m_LengthLoaded)
                        return null;
                }

                if(data.Total >= m_Length)
                {
                    rest = data.Total - m_Length;
                    NextReceiveFilter = m_SwitchReceiveFilter;
                    return new WebSocketPackageInfo(data, m_SwitchReceiveFilter.Context);
                }

                return null;
            }

            public void Reset()
            {
                m_Length = 0;
                m_LengthLoaded = false;
                State = FilterState.Normal;
            }
        }

        private IReceiveFilter<WebSocketPackageInfo> m_MarkReceiveFilter;
        private IReceiveFilter<WebSocketPackageInfo> m_LenReceiveFilter;
        private IReceiveFilter<WebSocketPackageInfo> m_CloseMarkReceiveFilter;

        public WebSocketContext Context { get; private set; }

        public DraftHybi00DataReceiveFilter(WebSocketContext context)
        {
            Context = context;
            m_MarkReceiveFilter = new MarkReceiveFilter(this);
            m_LenReceiveFilter = new LenReceiveFilter(this);
            m_CloseMarkReceiveFilter = new CloseMarkReceiveFilter(this);
        }

        public WebSocketPackageInfo Filter(BufferList data, out int rest)
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

        public IReceiveFilter<WebSocketPackageInfo> NextReceiveFilter { get; private set; }

        public FilterState State { get; private set; }

        public void Reset()
        {
            
        }
    }
}
