using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;

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

        /// <summary>
        /// Gets the status info metadata from the server type.
        /// </summary>
        /// <param name="serverType">Type of the server.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static StatusInfoAttribute[] GetStatusInfoMetadata(this Type serverType)
        {
            if (serverType == null)
                throw new ArgumentNullException("serverType");

            var attType = typeof(AppServerMetadataTypeAttribute);

            while (true)
            {
                var atts = serverType.GetCustomAttributes(attType, false);

                if (atts != null && atts.Length > 0)
                {
                    var serverMetadataTypeAtt = atts[0] as AppServerMetadataTypeAttribute;
                    return serverMetadataTypeAtt
                            .MetadataType
                            .GetCustomAttributes(typeof(StatusInfoAttribute), true)
                            .OfType<StatusInfoAttribute>().ToArray();
                }

                if (serverType.BaseType == null)
                    return null;

                serverType = serverType.BaseType;
            }
        }
    }
}
