using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// IHttpRequestInfo
    /// </summary>
    public interface IHttpRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Gets the http header.
        /// </summary>
        NameValueCollection Header { get; }
    }

    /// <summary>
    /// HttpRequestInfoBase
    /// </summary>
    public abstract class HttpRequestInfoBase : IHttpRequestInfo
    {
        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the http header.
        /// </summary>
        public NameValueCollection Header { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestInfoBase"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        protected HttpRequestInfoBase(string key, NameValueCollection header)
        {
            Key = key;
            Header = header;
        }
    }

    /// <summary>
    /// HttpRequestInfoBase
    /// </summary>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public abstract class HttpRequestInfoBase<TRequestBody> : HttpRequestInfoBase
    {
        /// <summary>
        /// Gets the body.
        /// </summary>
        public TRequestBody Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestInfoBase&lt;TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        protected HttpRequestInfoBase(string key, NameValueCollection header, TRequestBody body)
            : base(key, header)
        {
            Body = body;
        }
    }
}
