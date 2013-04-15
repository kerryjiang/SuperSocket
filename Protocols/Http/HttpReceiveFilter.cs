using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.Facility.Protocol;

namespace SuperSocket.Http
{
    public class HttpReceiveFilter : HttpReceiveFilterBase<HttpRequestInfo>
    {
        private long m_ContentLength;

        private NameValueCollection m_Header;

        protected override HttpRequestInfo FilterRequestBody(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            return NullRequestInfo;
        }

        protected override HttpRequestInfo FilterRequestHeader(NameValueCollection header)
        {
            var contentLength = header.Get(HttpHeaderKey.ContentLength);

            if (!string.IsNullOrEmpty(contentLength))
                long.TryParse(contentLength, out m_ContentLength);

            if (m_ContentLength > 0)
            {
                m_Header = header;
                return NullRequestInfo;
            }

            return new HttpRequestInfo("GET", header);
        }

        public override void Reset()
        {
            m_ContentLength = 0;
            m_Header = null;
            base.Reset();
        }
    }
}
