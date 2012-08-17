using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CountSpliterProtocol
{
    /// <summary>
    /// Your protocol likes like the format below:
    /// #part1#part2#part3#part4#part5#part6#part7#
    /// </summary>
    class MyRequetFilter : CountSpliterRequestFilter, IOffsetAdapter
    {
        public MyRequetFilter()
            : base((byte)'#', 8)
        {
            
        }
    }
}
