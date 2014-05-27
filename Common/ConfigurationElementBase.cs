using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

namespace SuperSocket.Common
{
    /// <summary>
    /// ConfigurationElementBase
    /// </summary>
    [Serializable]
    public class ConfigurationElementBase : ConfigurationElement
    {
        private bool m_NameRequired;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationElementBase"/> class.
        /// </summary>
        public ConfigurationElementBase()
            : this(true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationElementBase"/> class.
        /// </summary>
        /// <param name="nameRequired">if set to <c>true</c> [name required].</param>
        public ConfigurationElementBase(bool nameRequired)
        {
            m_NameRequired = nameRequired;
            Options = new NameValueCollection();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return this["name"] as string; }
        }

        /// <summary>
        /// Reads XML from the configuration file.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> that reads from the configuration file.</param>
        /// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false.</param>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element to read is locked.- or -An attribute of the current node is not recognized.- or -The lock status of the current node cannot be determined.  </exception>
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);

            if (m_NameRequired && string.IsNullOrEmpty(Name))
            {
                throw new ConfigurationErrorsException("Required attribute 'name' not found.");
            }
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public NameValueCollection Options { get; set; }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Options.Add(name, value);
            return true;
        }

        /// <summary>
        /// Modifies the <see cref="T:System.Configuration.ConfigurationElement" /> object to remove all values that should not be saved.
        /// </summary>
        /// <param name="sourceElement">A <see cref="T:System.Configuration.ConfigurationElement" /> at the current level containing a merged view of the properties.</param>
        /// <param name="parentElement">The parent <see cref="T:System.Configuration.ConfigurationElement" />, or null if this is the top level.</param>
        /// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> that determines which property values to include.</param>
        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);

            var element = sourceElement as ConfigurationElementBase;

            if (element == null)
                return;

            if (element.Options != this.Options)
                this.Options = element.Options;

            if (element.OptionElements != this.OptionElements)
                this.OptionElements = element.OptionElements;
        }

        /// <summary>
        /// Writes the contents of this configuration element to the configuration file when implemented in a derived class.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that writes to the configuration file.</param>
        /// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false.</param>
        /// <returns>
        /// true if any data was actually serialized; otherwise, false.
        /// </returns>
        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            if (!base.SerializeElement(writer, serializeCollectionKey))
                return false;

            if (writer == null)
                return true;

            var options = Options;

            if (options != null && options.Count > 0)
            {
                for (var i = 0; i < options.Count; i++)
                {
                    writer.WriteAttributeString(options.GetKey(i), options.Get(i));
                }
            }

            var optionElements = OptionElements;

            if (optionElements != null && optionElements.Count > 0)
            {
                for (var i = 0; i < optionElements.Count; i++)
                {
                    writer.WriteRaw(optionElements.Get(i));
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; set; }

        /// <summary>
        /// Gets a value indicating whether an unknown element is encountered during deserialization.
        /// </summary>
        /// <param name="elementName">The name of the unknown subelement.</param>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> being used for deserialization.</param>
        /// <returns>
        /// true when an unknown element is encountered while deserializing; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName"/> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName"/> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            if (OptionElements == null)
                OptionElements = new NameValueCollection();

            OptionElements.Add(elementName, reader.ReadOuterXml());
            return true;
        }
    }
}

