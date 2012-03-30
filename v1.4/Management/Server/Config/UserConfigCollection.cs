using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.Management.Server.Config
{
    [ConfigurationCollection(typeof(UserConfig))]
    public class UserConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new UserConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UserConfig)element).Name;
        }

        public new IEnumerator<UserConfig> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return (UserConfig)base.BaseGet(i);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "user";
            }
        }
    }
}
