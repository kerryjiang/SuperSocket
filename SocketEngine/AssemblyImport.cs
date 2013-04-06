using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// AssemblyImport, used for importing assembly to the current AppDomain
    /// </summary>
    public class AssemblyImport : MarshalByRefObject
    {
        private string m_ImportRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyImport"/> class.
        /// </summary>
        public AssemblyImport(string importRoot)
        {
            m_ImportRoot = importRoot;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        //Process cannot resolved assemblies
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);

            var assemblyFilePath = Path.Combine(m_ImportRoot, name.Name + ".dll");

            if (!File.Exists(assemblyFilePath))
                return null;

            return Assembly.LoadFrom(assemblyFilePath);
        }
    }
}
