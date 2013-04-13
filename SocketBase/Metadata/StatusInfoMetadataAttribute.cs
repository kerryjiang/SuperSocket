using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// StatusInfo Metadata Attribute
    /// </summary>
    public class StatusInfoMetadataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusInfoMetadataAttribute" /> class.
        /// </summary>
        /// <param name="metadataType">Type of the metadata.</param>
        public StatusInfoMetadataAttribute(Type metadataType)
        {
            MetadataType = metadataType;
        }

        /// <summary>
        /// Gets the type of the metadata.
        /// </summary>
        /// <value>
        /// The type of the metadata.
        /// </value>
        public Type MetadataType { get; private set; }
    }
}
