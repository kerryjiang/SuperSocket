using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// TypeProviderConfig
    /// </summary>
    [Serializable]
    public class TypeProviderConfig : ITypeProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public string Type { get; set; }
    }
}
