using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// IHttpRequestInfo
    /// </summary>
    public interface IHttpPackageInfo : IPackageInfo<string>
    {
        /// <summary>
        /// Gets the http header.
        /// </summary>
        HttpHeaderInfo Header { get; }
    }

    /// <summary>
    /// HttpRequestInfoBase
    /// </summary>
    public abstract class HttpPackageInfoBase : IHttpPackageInfo
    {
        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the http header.
        /// </summary>
        public HttpHeaderInfo Header { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPackageInfoBase"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        protected HttpPackageInfoBase(string key, HttpHeaderInfo header)
        {
            Key = key;
            Header = header;
        }
    }

    /// <summary>
    /// HttpPackageInfoBase
    /// </summary>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public abstract class HttpPackageInfoBase<TRequestBody> : HttpPackageInfoBase
    {
        /// <summary>
        /// Gets the body.
        /// </summary>
        public TRequestBody Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPackageInfoBase&lt;TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        protected HttpPackageInfoBase(string key, HttpHeaderInfo header, TRequestBody body)
            : base(key, header)
        {
            Body = body;
        }
    }
}
