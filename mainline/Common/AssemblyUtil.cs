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

        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
            where TBaseInterface : class
        {
            Type interfaceType = typeof(TBaseInterface);
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                var foundInterface = currentImplementType.GetInterfaces().SingleOrDefault(x => x == interfaceType);

                if (foundInterface != null)
                {
                    result.Add(currentImplementType.GetConstructor(new Type[0]).Invoke(new object[0]) as TBaseInterface);
                }
            }

            return result;
        }
    }
}
