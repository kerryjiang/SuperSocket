using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// StatusInfoMetadata type attribute
    /// </summary>
    public class AppServerMetadataTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the metadata.
        /// </summary>
        /// <value>
        /// The type of the metadata.
        /// </value>
        public Type MetadataType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerMetadataTypeAttribute" /> class.
        /// </summary>
        /// <param name="metadataType">Type of the metadata.</param>
        public AppServerMetadataTypeAttribute(Type metadataType)
        {
            MetadataType = metadataType;
        }
    }
}
