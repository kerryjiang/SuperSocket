using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.AgentClient.Metadata
{
    /// <summary>
    /// StateFieldMetadata
    /// </summary>
    public class StateFieldMetadata
    {
        /// <summary>
        /// Gets or sets the instance names which can be used for.
        /// </summary>
        /// <value>
        /// The instance names.
        /// </value>
        public List<string> InstanceNames { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public ClientFieldAttribute[] Fields { get; set; }
    }
}
