using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Reflection;

namespace SuperSocket.Common
{
    public static class GlobalResources
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        public static void Setup(string baseName, Assembly assembly)
        {
            resourceMan = new ResourceManager(baseName, assembly);
        }

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

        public static string GetString(string name, CultureInfo culture)
        {
            if (resourceMan != null)
                return resourceMan.GetString(name, culture);
            else
                return string.Empty;
        }

        public static string GetString(string name)
        {
            if (resourceMan != null)
                return GetString(name, resourceCulture);
            else
                return string.Empty;
        }
    }
}
