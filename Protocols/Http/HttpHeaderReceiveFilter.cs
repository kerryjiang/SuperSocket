using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.Http
{
    public class HttpHeaderReceiveFilter : HttpHeaderReceiveFilterBase<HttpRequestInfo>
    {
        protected override IReceiveFilter<HttpRequestInfo> GetBodyReceiveFilter(HttpHeaderInfo header, int headerSize)
        {
            var contentLength = 0;

            if (int.TryParse(header.Get(HttpHeaderKey.ContentLength), out contentLength) && contentLength > 0)
                return new HttpBodyReceiveFilter(header, headerSize, contentLength);

            if (contentLength > 0)
            {
                return new HttpBodyReceiveFilter(header, headerSize, contentLength);
            }

            return null;
        }

        protected override HttpRequestInfo ResolveHttpRequestWithoutBody(HttpHeaderInfo header)
        {
            return new HttpRequestInfo(header.Path, header, string.Empty);
        }
    }
}
