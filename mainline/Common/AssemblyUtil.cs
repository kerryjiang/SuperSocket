using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;

#if SILVERLIGHT
#else
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace SuperSocket.Common
{
    public static class AssemblyUtil
    {
        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, new object[0]);
        }

        public static T CreateInstance<T>(string type, object[] parameters)
        {
            Type instanceType = null;
            var result = default(T);

            instanceType = Type.GetType(type, true);

            if (instanceType == null)
                throw new Exception(string.Format("The type '{0}' was not found!", type));

            object instance = Activator.CreateInstance(instanceType, parameters);
            result = (T)instance;
            return result;
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

#if SILVERLIGHT
#else
        public static T BinaryClone<T>(this T target)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, target);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
#endif

        private static object[] m_EmptyObjectArray = new object[] { };

        public static void CopyPropertiesTo(this object source, object target)
        {
            PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

            PropertyInfo[] targetProperties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            for (int i = 0; i < targetProperties.Length; i++)
            {
                var p = targetProperties[i];
                PropertyInfo sourceProperty;
                if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
                {
                    if (sourceProperty.PropertyType != p.PropertyType)
                        continue;
                }

                p.SetValue(target, sourceProperty.GetValue(source, m_EmptyObjectArray), m_EmptyObjectArray);
            }
        }

        public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
        {
            string[] assemblies = assemblyDef.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<Assembly> result = new List<Assembly>(assemblies.Length);

            foreach (var a in assemblies)
            {
                result.Add(Assembly.Load(a));
            }

            return result;
        }
    }
}
