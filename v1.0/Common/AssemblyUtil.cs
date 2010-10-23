using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;

namespace SuperSocket.Common
{
    public static class AssemblyUtil
    {
        public static bool TryCreateInstance<T>(string type, out T result)
        {
            Type instanceType = null;
            result = default(T);

            if (!TryGetType(type, out instanceType))
                return false;

            try
            {
                object instance = Activator.CreateInstance(instanceType);
                result = (T)instance;
                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError("TryCreateInstance" , e);
                return false;
            }
        }

        public static bool TryGetType(string type, out Type result)
        {
            try
            {
                result = Type.GetType(type);
                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                result = null;
                return false;
            }
        }

        public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(t =>
                t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
        }
    }
}
