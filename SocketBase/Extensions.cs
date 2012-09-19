using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static IWorkItem GetServerByName(this IBootstrap bootstrap, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return bootstrap.AppServers.FirstOrDefault(s => name.Equals(s.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
