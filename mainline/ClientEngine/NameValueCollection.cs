using System;
using System.Linq;
using System.Collections.Generic;

namespace SuperSocket.ClientEngine
{
    public class NameValueCollection : List<KeyValuePair<string, string>>
    {
        public new string this[int index]
        {
            get
            {
                return base[index].Value;
            }
        }

        public string this[string name]
        {
            get
            {
                return this.SingleOrDefault(kv => kv.Key.Equals(name)).Value;
            }
        }

        public NameValueCollection()
        {

        }

        public NameValueCollection(int capacity)
            : base(capacity)
        {

        }

        public void Add(string name, string value)
        {
            List<KeyValuePair<string, string>> list = this;
            for (int i = Count - 1; i >= 0; --i)
            {
                if (string.Equals(list[i].Key, name))
                {
                    list[i] = new KeyValuePair<string, string>(name, list[i].Value + "," + value);
                    return;
                }
            }

            Add(new KeyValuePair<string, string>(name, value));
        }

        public IEnumerable<string> AllKeys
        {
            get
            {
                return this.Select(pair => pair.Key);
            }
        }
    }
}
