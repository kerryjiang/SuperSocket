using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// RequestInfo basic class
    /// </summary>
    /// <typeparam name="TRequestData">The type of the request data.</typeparam>
    public class RequestInfo<TRequestData> : IRequestInfo<TRequestData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInfo&lt;TRequestData&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        protected RequestInfo(string key, TRequestData data)
        {
            Key = key;
            Data = data;
        }

        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public TRequestData Data { get; private set; }
    }
}
