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

        public string StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        public NameValueCollection Items { get; private set; }

        private HttpHeader()
        {
            
        }

        public static HttpHeader CreateForRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            return new HttpHeader
            {
                Method = method,
                Path = path,
                HttpVersion = httpVersion,
                Items = items
            };
        }

        public static HttpHeader CreateForResponse(string httpVersion, string statusCode, string statusDescription, NameValueCollection items)
        {
            return new HttpHeader
            {
                HttpVersion = httpVersion,
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                Items = items
            };
        }
    }
}
