using System;
using System.Collections.Generic;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Modularity.PlugIns;

namespace Super.Host
{
    public class AssemblyNamePlugInSource : IPlugInSource
    {
        private readonly HashSet<string> _assemblyNames;
       
        public AssemblyNamePlugInSource(List<string> assemblyNames)
        {
            _assemblyNames = new HashSet<string>();

            foreach (var assemblyName in assemblyNames)
            {
                _assemblyNames.Add(assemblyName);
            }
        }
      
        public Type[] GetModules()
        {
            var modules = new List<Type>();

            foreach (var assemblyName in _assemblyNames)
            {
                var assembly = Assembly.Load(assemblyName);

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (AbpModule.IsAbpModule(type))
                        {
                            modules.AddIfNotContains(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new AbpException("Could not get module types from assembly: " + assembly.FullName, ex);
                }
            }

            return modules.ToArray();
        }
    }
}
