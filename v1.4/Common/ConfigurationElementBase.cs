using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

namespace SuperSocket.Common
{
    public class ConfigurationElementBase : ConfigurationElement
    {
        private bool m_NameRequired;

        public ConfigurationElementBase()
            : this(true)
        {

        }

        public ConfigurationElementBase(bool nameRequired)
        {
            m_NameRequired = nameRequired;
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get { return this["name"] as string; }
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);

            if (m_NameRequired && string.IsNullOrEmpty(Name))
            {
                throw new ConfigurationErrorsException("Required attribute 'name' not found.");
            }
        }

        public NameValueCollection Options { get; private set; }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (Options == null)
            {
                Options = new NameValueCollection();
            }

            Options.Add(name, value);
            return true;
        }

        protected NameValueCollection OptionElements { get; private set; }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            if (OptionElements == null)
                OptionElements = new NameValueCollection();

            OptionElements.Add(elementName, reader.ReadOuterXml());
            return true;
        }
    }
}

