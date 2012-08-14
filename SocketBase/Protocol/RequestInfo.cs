using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// RequestInfo basic class
    /// </summary>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public class RequestInfo<TRequestBody> : IRequestInfo<TRequestBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestBody&gt;"/> class.
        /// </summary>
        protected RequestInfo()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        public RequestInfo(string key, TRequestBody body)
        {
            Initialize(key, body);
        }

        /// <summary>
        /// Initializes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        protected void Initialize(string key, TRequestBody body)
        {
            Key = key;
            Body = body;
        }

        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        public TRequestBody Body { get; private set; }
    }

    /// <summary>
    /// RequestInfo with header
    /// </summary>
    /// <typeparam name="TRequestHeader">The type of the request header.</typeparam>
    /// <typeparam name="TRequestBody">The type of the request body.</typeparam>
    public class RequestInfo<TRequestHeader, TRequestBody> : RequestInfo<TRequestBody>, IRequestInfo<TRequestHeader, TRequestBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestHeader, TRequestBody&gt;"/> class.
        /// </summary>
        public RequestInfo()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestHeader, TRequestBody&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public RequestInfo(string key, TRequestHeader header, TRequestBody body)
            : base(key, body)
        {
            Header = header;
        }

        /// <summary>
        /// Initializes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public void Initialize(string key, TRequestHeader header, TRequestBody body)
        {
            base.Initialize(key, body);
            Header = header;
        }
        /// <summary>
        /// Gets the header.
        /// </summary>
        public TRequestHeader Header { get; private set; }
    }
}
