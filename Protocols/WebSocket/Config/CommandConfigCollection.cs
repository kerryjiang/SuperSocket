using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.WebSocket.Config
{
    /// <summary>
    /// Command configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(CommandConfig))]
    public class CommandConfigCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <returns>The specified property, attribute, or child element</returns>
        public CommandConfig this[int index]
        {
            get
            {
                return (CommandConfig)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CommandConfig();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return element.GetHashCode();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<CommandConfig> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (CommandConfig)base.BaseGet(i);
            }
        }
    }
}
