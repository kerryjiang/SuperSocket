using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Globalization;

#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace SuperSocket.Common
{
    /// <summary>
    /// Assembly Util Class
    /// </summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// Creates the instance from type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, new object[0]);
        }

        /// <summary>
        /// Creates the instance from type name and parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the type by the full name, also return matched generic type without checking generic type parameters in the name.
        /// </summary>
        /// <param name="fullTypeName">Full name of the type.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
#if !NET35
        public static Type GetType(string fullTypeName, bool throwOnError, bool ignoreCase)
        {
            var targetType = Type.GetType(fullTypeName, false, ignoreCase);

            if (targetType != null)
                return targetType;

            var names = fullTypeName.Split(',');
            var assemblyName = names[1].Trim();

            try
            {
                var assembly = Assembly.Load(assemblyName);

                var typeNamePrefix = names[0].Trim() + "`";

                var matchedTypes = assembly.GetExportedTypes().Where(t => t.IsGenericType
                        && t.FullName.StartsWith(typeNamePrefix, ignoreCase, CultureInfo.InvariantCulture)).ToArray();

                if (matchedTypes.Length != 1)
                    return null;

                return matchedTypes[0];
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw e;

                return null;
            }
        }
#else
        public static Type GetType(string fullTypeName, bool throwOnError, bool ignoreCase)
        {
            return Type.GetType(fullTypeName, null, (a, n, ign) =>
                {
                    var targetType = a.GetType(n, false, ign);

                    if (targetType != null)
                        return targetType;

                    var typeNamePrefix = n + "`";

                    var matchedTypes = a.GetExportedTypes().Where(t => t.IsGenericType
                            && t.FullName.StartsWith(typeNamePrefix, ign, CultureInfo.InvariantCulture)).ToArray();

                    if (matchedTypes.Length != 1)
                        return null;

                    return matchedTypes[0];
                }, throwOnError, ignoreCase);
        }
#endif

        /// <summary>
        /// Gets the implement types from assembly.
        /// </summary>
        /// <typeparam name="TBaseType">The type of the base type.</typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(t =>
                t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
        }

        /// <summary>
        /// Gets the implemented objects by interface.
        /// </summary>
        /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
            where TBaseInterface : class
        {
            return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
        }

        /// <summary>
        /// Gets the implemented objects by interface.
        /// </summary>
        /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
            where TBaseInterface : class
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;

                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }

#if SILVERLIGHT
#else
        /// <summary>
        /// Clone object in binary format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Copies the properties of one object to another object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static T CopyPropertiesTo<T>(this T source, T target)
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

                    if (!sourceProperty.PropertyType.IsSerializable)
                        continue;

                    p.SetValue(target, sourceProperty.GetValue(source, m_EmptyObjectArray), m_EmptyObjectArray);
                }
            }

            return target;
        }

        /// <summary>
        /// Gets the assemblies from string.
        /// </summary>
        /// <param name="assemblyDef">The assembly def.</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
        {
            return GetAssembliesFromStrings(assemblyDef.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Gets the assemblies from strings.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesFromStrings(string[] assemblies)
        {
            List<Assembly> result = new List<Assembly>(assemblies.Length);

            foreach (var a in assemblies)
            {
                result.Add(Assembly.Load(a));
            }

            return result;
        }
    }
}
