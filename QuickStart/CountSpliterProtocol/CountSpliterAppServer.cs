using System;
using System.Text;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CountSpliterProtocol
{
    /// <summary>
    /// Your protocol likes like the format below:
    /// #part1#part2#part3#part4#part5#part6#part7#
    /// </summary>
    public class CountSpliterAppServer : AppServer
    {
        public CountSpliterAppServer()
            : base(new CountSpliterReceiveFilterFactory((byte)'#', 8))
        {
            
        }
    }
}
