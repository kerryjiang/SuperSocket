using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Test.Udp
{
    class MyRequestFilter : IRequestFilter<MyUdpRequestInfo>
    {
        public MyUdpRequestInfo Filter(IAppSession<MyUdpRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;

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

        public IRequestFilter<MyUdpRequestInfo> NextRequestFilter
        {
            get { return this; }
        }
    }
}
