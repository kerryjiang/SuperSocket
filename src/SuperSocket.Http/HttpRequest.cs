using System;
using System.Collections.Specialized;

namespace SuperSocket.Http
{
    public class HttpRequest
    {
        public string Method { get; private set; }

        public string Path { get; private set; }

        public string HttpVersion { get; private set; }

        public NameValueCollection Items { get; private set; }

        public string Body { get; set; }

        public HttpRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            Method = method;
            Path = path;
            HttpVersion = httpVersion;
            Items = items;
        }
    }
}
