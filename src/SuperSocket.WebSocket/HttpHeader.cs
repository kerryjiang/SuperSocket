using System;
using System.Buffers;
using System.Collections.Specialized;

namespace SuperSocket.WebSocket
{
    public class HttpHeader
    {
        public string Method { get; private set; }

        public string Path { get; private set; }

        public string HttpVersion { get; private set; }

        public NameValueCollection Items { get; private set; }

        public HttpHeader(string method, string path, string httpVersion, NameValueCollection items)
        {
            Method = method;
            Path = path;
            HttpVersion = httpVersion;
            Items = items;
        }
    }
}
