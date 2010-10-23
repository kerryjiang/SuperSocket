using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Configuration
{
    [ConfigurationCollection(typeof(Server), AddItemName = "server")] 
    public class ServerCollection : ConfigurationElementCollection
    {
        public Server this[int index]
        {
            get
            {
                return base.BaseGet(index) as Server;
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
            return new Server();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Server)element).Name;
        }
    }
}
