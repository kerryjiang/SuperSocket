using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Resources;

namespace SuperSocket.Common
{
    public static class GlobalResourceManager
    {
        private static Dictionary<string, ResourceManager> m_DictResources = new Dictionary<string, ResourceManager>();

        private static object m_SyncRoot = new object();

        public static void RegisterResource(string name, string baseName, Assembly assembly)
        {
            ResourceManager resource = new ResourceManager(baseName, assembly);

            lock (m_SyncRoot)
            {
                m_DictResources[name.ToLower()] = resource;
            }
        }

        public static string GetString(string resourceName, string key)
        {
            ResourceManager manager;

            if (m_DictResources.TryGetValue(resourceName.ToLower(), out manager))
            {
                return manager.GetString(key);
            }
            else
            {
                return string.Empty;
            }
        }

        public static ResourceManager GetResourceManager(string resourceName)
        {
            ResourceManager manager;

            if (m_DictResources.TryGetValue(resourceName.ToLower(), out manager))
            {
                return manager;
            }
            else
            {
                return null;
            }
        }
    }
}
