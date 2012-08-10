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
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        protected RequestInfo(string key, TRequestBody body)
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
}
