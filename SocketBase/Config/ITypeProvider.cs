using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// TypeProvider's interface
    /// </summary>
    public interface ITypeProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        string Type { get; }
    }
}
