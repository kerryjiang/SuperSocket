using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace GiantSoft.Common
{
    public static class AssemblyUtil
    {
        public static bool TryCreateInstance<T>(string assembly, out T result)
        {
            string[] arrAssembly = assembly.Split(',');
            string className = arrAssembly[0];
            string assemblyPath = arrAssembly[1] + ".dll";

            Assembly ass = Assembly.LoadFrom(assemblyPath);

            if (ass != null)
            {
                object instance = ass.CreateInstance(className, true);

                if (instance != null)
                {
                   result = (T)instance;
                   return true;
                }
            }

            result = default(T);
            return false;
        }

        public static bool TryGetType<T>(string assembly, out Type result)
        {
            result = null;            

            string[] arrAssembly    = assembly.Split(',');
            string className        = arrAssembly[0].Trim();
            string assemblyPath     = arrAssembly[1].Trim() + ".dll";

            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Assembly ass = Assembly.LoadFrom(Path.Combine(currentPath, assemblyPath));

            if (ass != null)
            {
                result = ass.GetType(className, false, true);

                if (result != null)
                    return true;
                else
                {
                    LogUtil.LogError("Failed to load type " + className);
                    return false;
                }
            }
            else
            {
                LogUtil.LogError("Failed to load assembly " + assemblyPath);
                return false;
            }
        }
    }
}
