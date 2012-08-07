using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Provider
{
    /// <summary>
    /// Provider factory infomation
    /// </summary>
    [Serializable]
    public class ProviderFactoryInfo
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        public ProviderKey Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the export factory.
        /// </summary>
        /// <value>
        /// The export factory.
        /// </value>
        public ExportFactory ExportFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactoryInfo"/> class.
        /// </summary>
        public ProviderFactoryInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactoryInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="instance">The instance.</param>
        public ProviderFactoryInfo(ProviderKey key, string name, object instance)
        {
            Key = key;
            Name = name;
            ExportFactory = new ExportFactory(instance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactoryInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="typeName">Name of the type.</param>
        public ProviderFactoryInfo(ProviderKey key, string name, string typeName)
        {
            Key = key;
            Name = name;
            ExportFactory = new ExportFactory(typeName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderFactoryInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public ProviderFactoryInfo(ProviderKey key, string name, Type type)
            : this(key, name, type.AssemblyQualifiedName)
        {
            
        }
    }
}
