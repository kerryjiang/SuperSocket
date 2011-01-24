using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.Common
{
    public static class ConfigurationExtension
    {
        public static string GetValue(this NameValueConfigurationCollection collection, string key)
        {
            return GetValue(collection, key, string.Empty);
        }

        public static string GetValue(this NameValueConfigurationCollection collection, string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (collection == null)
                return defaultValue;

            var e = collection[key];

            if (e == null)
                return defaultValue;

            return e.Value;
        }
    }
}
