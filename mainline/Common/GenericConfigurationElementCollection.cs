using System;
using System.Configuration;
using System.Collections.Generic;

namespace SuperSocket.Common
{
    public class GenericConfigurationElementCollectionBase<TConfigElement, TConfigInterface> : ConfigurationElementCollection, IEnumerable<TConfigInterface>
        where TConfigElement : ConfigurationElement, TConfigInterface, new()
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

        protected override ConfigurationElement CreateNewElement()
        {
            return new TConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            throw new NotImplementedException();
        }

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

    public class GenericConfigurationElementCollection<TConfigElement, TConfigInterface> : GenericConfigurationElementCollectionBase<TConfigElement, TConfigInterface>, IEnumerable<TConfigInterface>
        where TConfigElement : ConfigurationElementBase, TConfigInterface, new()
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TConfigElement)element).Name;
        }
    }
}

