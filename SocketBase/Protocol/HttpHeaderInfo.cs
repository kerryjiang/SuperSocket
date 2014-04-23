using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace SuperSocket.SocketBase.Protocol
{
    public class HttpHeaderInfo : NameValueCollection
    {
        public string Method { get; internal set; }

        public string Path { get; internal set; }

        public string Version { get; internal set; }
    }
}
