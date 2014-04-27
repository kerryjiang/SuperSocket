using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Http
{
    /// <summary>
    /// HttpRequestInfo
    /// </summary>
    public class HttpPackageInfo : HttpPackageInfoBase
    {
        /// <summary>
        /// Gets the http method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; private set; }

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <value>
        /// The cookies.
        /// </value>
        public NameValueCollection Cookies { get; private set; }

        /// <summary>
        /// Gets the form.
        /// </summary>
        /// <value>
        /// The form.
        /// </value>
        public NameValueCollection Form { get; private set; }


        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPackageInfo" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        public HttpPackageInfo(string key, HttpHeaderInfo header, string body)
            : base(key, header)
        {
            Body = body;
            Method = header.Method;
        }
    }
}
