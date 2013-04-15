using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using System.Configuration;

namespace SuperSocket.WebSocket.Config
{
    /// <summary>
    /// SubProtocol configuration
    /// </summary>
    public class SubProtocolConfig : ConfigurationElementBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubProtocolConfig"/> class.
        /// </summary>
        public SubProtocolConfig()
            : base(false)
        {

        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
        }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        [ConfigurationProperty("commands")]
        public CommandConfigCollection Commands
        {
            get
            {
                return this["commands"] as CommandConfigCollection;
            }
        }
    }
}
