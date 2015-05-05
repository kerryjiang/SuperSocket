using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase.Provider
{
    /// <summary>
    /// Provider metadata attribute
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ProviderMetadataAttribute : ExportAttribute, IProviderMetadata
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderMetadataAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        public ProviderMetadataAttribute(string name)
        {
            Name = name;
        }
    }
}
