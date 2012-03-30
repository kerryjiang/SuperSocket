using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Resources;

namespace SuperSocket.Common
{
    /// <summary>
    /// Global resource manager
    /// </summary>
    public static class GlobalResourceManager
    {
        private static Dictionary<string, ResourceManager> m_DictResources = new Dictionary<string, ResourceManager>();

        private static object m_SyncRoot = new object();

        /// <summary>
        /// Registers the resource into resource manager.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="assembly">The assembly.</param>
        public static void RegisterResource(string name, string baseName, Assembly assembly)
        {
            ResourceManager resource = new ResourceManager(baseName, assembly);

            lock (m_SyncRoot)
            {
                m_DictResources[name.ToLower()] = resource;
            }
        }

        /// <summary>
        /// Gets the string from resource manager by resource name and key.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the resource manager by name.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
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
