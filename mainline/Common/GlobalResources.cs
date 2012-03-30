using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Reflection;

namespace SuperSocket.Common
{
    /// <summary>
    /// Global resources
    /// </summary>
    public static class GlobalResources
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        /// <summary>
        /// Setups by the specified base name.
        /// </summary>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="assembly">The assembly.</param>
        public static void Setup(string baseName, Assembly assembly)
        {
            resourceMan = new ResourceManager(baseName, assembly);
        }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        public static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static string GetString(string name, CultureInfo culture)
        {
            if (resourceMan != null)
                return resourceMan.GetString(name, culture);
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string GetString(string name)
        {
            if (resourceMan != null)
                return GetString(name, resourceCulture);
            else
                return string.Empty;
        }
    }
}
