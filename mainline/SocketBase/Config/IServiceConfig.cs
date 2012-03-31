using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// The service interface
    /// </summary>
    public interface IServiceConfig
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        NameValueCollection Options { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IServiceConfig"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        bool Disabled { get; }
    }
}
