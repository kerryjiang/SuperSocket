using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyReceiveFilter : IReceiveFilter<MyUdpRequestInfo>
    {
        public MyUdpRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;

            if (length <= 40)
                return null;

            var key = Encoding.ASCII.GetString(readBuffer, offset, 4);
            var sessionID = Encoding.ASCII.GetString(readBuffer, offset + 4, 36);

            var data = Encoding.UTF8.GetString(readBuffer, offset + 40, length - 40);

            return new MyUdpRequestInfo(key, sessionID) { Value = data };
        }

        public int LeftBufferSize
        {
            get { return 0; }
        }

        public IReceiveFilter<MyUdpRequestInfo> NextReceiveFilter
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the filter state.
        /// </summary>
        /// <value>
        /// The filter state.
        /// </value>
        public FilterState State { get; private set; }

        public void Reset()
        {
            
        }
    }
}
