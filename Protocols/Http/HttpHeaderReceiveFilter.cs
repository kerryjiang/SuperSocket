using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.Http
{
    public class HttpHeaderReceiveFilter : HttpHeaderReceiveFilterBase<HttpPackageInfo>
    {
        protected override IReceiveFilter<HttpPackageInfo> GetBodyReceiveFilter(HttpHeaderInfo header, int headerSize)
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

        protected override HttpPackageInfo ResolveHttpPackageWithoutBody(HttpHeaderInfo header)
        {
            return new HttpPackageInfo(header.Path, header, string.Empty);
        }
    }
}
