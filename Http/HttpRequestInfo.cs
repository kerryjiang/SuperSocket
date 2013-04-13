using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SuperSocket.Facility.Protocol;

namespace SuperSocket.Http
{
    /// <summary>
    /// HttpRequestInfo
    /// </summary>
    public class HttpRequestInfo : HttpRequestInfoBase
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
        /// Initializes a new instance of the <see cref="HttpRequestInfo" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="header">The header.</param>
        public HttpRequestInfo(string key, NameValueCollection header)
            : base(key, header)
        {
            
        }
    }
}
