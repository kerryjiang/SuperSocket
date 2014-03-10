using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using System.Configuration;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Buffer pool configuration
    /// </summary>
    public class BufferPoolConfig : ConfigurationElement, IBufferPoolConfig
    {
        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        /// <value>
        /// The size of the buffer.
        /// </value>
        [ConfigurationProperty("bufferSize", IsRequired = true)]
        public int BufferSize
        {
            get
            {
                return (int)this["bufferSize"];
            }
        }

        /// <summary>
        /// Gets the initial count.
        /// </summary>
        /// <value>
        /// The initial count.
        /// </value>
        [ConfigurationProperty("initialCount", IsRequired = true)]
        public int InitialCount
        {
            get
            {
                return (int)this["initialCount"];
            }
        }
    }

    /// <summary>
    /// Buffer pool configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(CommandAssembly))]
    public class BufferPoolConfigCollection : GenericConfigurationElementCollectionBase<BufferPoolConfig, IBufferPoolConfig>
    {

    }
}
