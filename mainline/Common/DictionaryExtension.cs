using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public static class DictionaryExtension
    {
        public static T GetValue<T>(this IDictionary<object, object> dictionary, object key)
            where T : new()
        {
            T defaultValue = default(T);
            return GetValue<T>(dictionary, key, defaultValue);
        }

        public static T GetValue<T>(this IDictionary<object, object> dictionary, object key, T defaultValue)
        {
            object valueObj;

            if (!dictionary.TryGetValue(key, out valueObj))
            {
                return defaultValue;
            }
            else
            {
                return (T)valueObj;
            }
        }
    }
}
