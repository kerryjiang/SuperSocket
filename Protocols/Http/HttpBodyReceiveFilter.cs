using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using System.Collections.Specialized;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Http
{
    public class HttpBodyReceiveFilter : FixedSizeReceiveFilter<HttpPackageInfo>
    {
        private HttpHeaderInfo m_Header;
        private int m_HeaderSize;

        public HttpBodyReceiveFilter(HttpHeaderInfo header, int headerSize, int bodyLength)
            : base(bodyLength)
        {
            m_Header = header;
            m_HeaderSize = headerSize;
        }

        public override HttpPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var body = Encoding.UTF8.GetString(packageData, m_HeaderSize, packageData.Sum(d => d.Count) - m_HeaderSize);
            return new HttpPackageInfo(m_Header.Path, m_Header, body);
        }
    }
}
