using System;
using System.Configuration;
using System.Collections.Specialized;

namespace SuperSocket.Common
{
    public class ConfigurationElementBase : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
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
    }
}

