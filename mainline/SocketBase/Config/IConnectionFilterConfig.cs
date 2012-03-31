using System;
using System.Configuration;
using System.Collections.Specialized;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Connection filter configuraion interface
    /// </summary>
    public interface IConnectionFilterConfig
    {
        /// <summary>
        /// Gets the filter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the filter's type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the configuration options of this filter.
        /// </summary>
        NameValueCollection Options { get; }
    }
}

