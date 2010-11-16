using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Configuration
{
    [ConfigurationCollection(typeof(ProtocolConfig), AddItemName = "protocol")]
    public class ProtocolCollectionConfig : ConfigurationElementCollection
    {
        public ProtocolConfig this[int index]
        {
            get
            {
                return base.BaseGet(index) as ProtocolConfig;
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

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProtocolConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProtocolConfig)element).Name;
        }
    }
}
