using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Http header info
    /// </summary>
    public class HttpHeaderInfo : Dictionary<string, string>
    {
        /// <summary>
        /// default constructor of HttpHeaderInfo
        /// </summary>
        public HttpHeaderInfo()
            : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public string Method { get; internal set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; internal set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; internal set; }

        /// <summary>
        /// Get item from header with key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var value = string.Empty;
            TryGetValue(key, out value);
            return value;
        }
    }
}
