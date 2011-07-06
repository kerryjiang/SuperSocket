using System;
using System.Configuration;
using System.Collections.Generic;

namespace SuperSocket.Common
{
    public class GenericConfigurationElementCollection<TConfigElement, TConfigInterface> : ConfigurationElementCollection, IEnumerable<TConfigInterface>
        where TConfigElement : ConfigurationElementBase, TConfigInterface, new()
    {
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
                this.BaseAdd(index, value);
            }
        }
        
        #region implemented abstract members of System.Configuration.ConfigurationElementCollection
        protected override ConfigurationElement CreateNewElement ()
        {
            return new TConfigElement();
        }
         
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TConfigElement)element).Name;
        }         
        
        #endregion
        
        #region IEnumerable[T] implementation
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
}

