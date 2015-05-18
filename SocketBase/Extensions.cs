using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Metadata;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Extensions class for SocketBase project
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the app server instance in the bootstrap by name, ignore case
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="name">The name of the appserver instance.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IManagedApp GetServerByName(this IBootstrap bootstrap, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return bootstrap.AppServers.FirstOrDefault(s => name.Equals(s.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Creates the default pool item creator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool">The pool.</param>
        /// <returns></returns>
        public static IPoolItemCreator<T> CreateDefaultPoolItemCreator<T>(this IPool<T> pool)
            where T : new()
        {
            return new DefaultConstructorItemCreator<T>();
        }

        /// <summary>
        /// Gets the command key from the command instance.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Command key definition was not found.</exception>
        public static object GetCommandKey<TKey>(this ICommand command)
        {
            var cmdAtt = command.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault();

            if (cmdAtt != null)
                return cmdAtt.Key;

            if (typeof(TKey) != typeof(string))
                throw new Exception("Command key definition was not found.");

            return command.Name;
        }

        private const string CurrentAppDomainExportProviderKey = "CurrentAppDomainExportProvider";

        /// <summary>
        /// Gets the current application domain's export provider.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <returns></returns>
        public static ExportProvider GetCurrentAppDomainExportProvider(this AppDomain appDomain)
        {
            var exportProvider = appDomain.GetData(CurrentAppDomainExportProviderKey) as ExportProvider;

            if (exportProvider != null)
                return exportProvider;

            var isolation = IsolationMode.None;
            var isolationValue = appDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue != null)
                isolation = (IsolationMode)isolationValue;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IAppServer).Assembly));

            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.*"));

            if (isolation != IsolationMode.None)
            {
                catalog.Catalogs.Add(new DirectoryCatalog(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "*.*"));
            }

            exportProvider = new CompositionContainer(catalog);

            appDomain.SetData(CurrentAppDomainExportProviderKey, exportProvider);

            return exportProvider;
        }
    }
}
