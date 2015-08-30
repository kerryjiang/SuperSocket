using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class MultipleProtocolSwitchReceiveFilter : IReceiveFilter<WebSocketPackageInfo>, IWebSocketReceiveFilter
    {
        public WebSocketContext Context { get; private set; }

        private static byte[] m_SwitchResponse;

        public MultipleProtocolSwitchReceiveFilter(byte[] switchResponse)
        {
            m_SwitchResponse = switchResponse;
        }

        public IReceiveFilter<WebSocketPackageInfo> NextReceiveFilter { get; private set; }

        public FilterState State { get; private set; }

        public WebSocketPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;
            return null;
        }

        public bool Handshake(WebSocketContext context)
        {
            Context = context;
            return true;
        }

        public void Reset()
        {
            
        }
    }
}
