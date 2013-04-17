using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.Test.Config
{
    public class ChildConfig : ConfigurationElement
    {
        [ConfigurationProperty("value", IsRequired = true)]
        public int Value
        {
            get { return (int)this["value"]; }
        }
    }

    [ConfigurationCollection(typeof(ChildConfig), AddItemName = "child")]
    public class ChildConfigCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("globalValue", IsRequired = true)]
        public int GlobalValue
        {
            get { return (int)this["globalValue"]; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ChildConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        public new IEnumerator<ChildConfig> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (ChildConfig)base.BaseGet(i);
            }
        }
    }
}
