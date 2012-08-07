using System;
using System.Configuration;
using System.Collections.Generic;

namespace SuperSocket.Common
{
    /// <summary>
    /// GenericConfigurationElementCollectionBase
    /// </summary>
    /// <typeparam name="TConfigElement">The type of the config element.</typeparam>
    /// <typeparam name="TConfigInterface">The type of the config interface.</typeparam>
    public class GenericConfigurationElementCollectionBase<TConfigElement, TConfigInterface> : ConfigurationElementCollection, IEnumerable<TConfigInterface>
        where TConfigElement : ConfigurationElement, TConfigInterface, new()
    {
        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <returns>The specified property, attribute, or child element</returns>
        public TConfigElement this[int index]
        {
            get
            {
                return (TConfigElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value as ConfigurationElement);
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
            return new TConfigElement() as ConfigurationElement;
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
            return element;
        }

        #region IEnumerable[T] implementation

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public new IEnumerator<TConfigInterface> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (TConfigElement)base.BaseGet(i);
            }
        }

        #endregion
    }

    /// <summary>
    /// GenericConfigurationElementCollection
    /// </summary>
    /// <typeparam name="TConfigElement">The type of the config element.</typeparam>
    /// <typeparam name="TConfigInterface">The type of the config interface.</typeparam>
    public class GenericConfigurationElementCollection<TConfigElement, TConfigInterface> : GenericConfigurationElementCollectionBase<TConfigElement, TConfigInterface>, IEnumerable<TConfigInterface>
        where TConfigElement : ConfigurationElementBase, TConfigInterface, new()
    {
        /// <summary>
        /// Gets the element key.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TConfigElement)element).Name;
        }
    }
}

