using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SuperSocket.Common
{
    /// <summary>
    /// Configuration extension class
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Gets the value from namevalue collection by key.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValue(this NameValueCollection collection, string key)
        {
            return GetValue(collection, key, string.Empty);
        }

        /// <summary>
        /// Gets the value from namevalue collection by key.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetValue(this NameValueCollection collection, string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (collection == null)
                return defaultValue;

            var e = collection[key];

            if (e == null)
                return defaultValue;

            return e;
        }

        /// <summary>
        /// Deserializes the specified configuration section.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="section">The section.</param>
        /// <param name="reader">The reader.</param>
        public static void Deserialize<TElement>(this TElement section, XmlReader reader)
            where TElement : ConfigurationElement
        {
            var deserializeElementMethod = typeof(TElement).GetMethod("DeserializeElement", BindingFlags.NonPublic | BindingFlags.Instance);
            deserializeElementMethod.Invoke(section, new object[] { reader, true });
        }
    }
}
