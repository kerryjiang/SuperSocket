using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyReceiveFilter : IReceiveFilter<MyUdpRequestInfo>
    {
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

        public MyUdpRequestInfo Filter(BufferList data, out int rest)
        {
            rest = 0;

            var length = data.Total;

            if (length <= 40)
                return null;

            var key = Encoding.ASCII.GetString(data, 0, 4);
            var sessionID = Encoding.ASCII.GetString(data, 4, 36);

            var body = Encoding.UTF8.GetString(data, 40, length - 40);

            return new MyUdpRequestInfo(key, sessionID) { Value = body };
        }
    }
}
