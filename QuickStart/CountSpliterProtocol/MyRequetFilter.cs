using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CountSpliterProtocol
{
    class MyRequetFilter : CountSpliterRequestFilter, IOffsetAdapter
    {
        public MyRequetFilter()
            : base((byte)'#', 8)
        {
            
        }
    }
}
