using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;
using System.Reflection;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Metadata Extensions
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets all status info atttributes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static StatusInfoAttribute[] GetAllStatusInfoAtttributes(this IStatusInfoSource source)
        {
            var atts = new List<StatusInfoAttribute>();

            var metadatas = source.GetType().GetCustomAttributes(typeof(StatusInfoMetadataAttribute), true);

            var statusInfoAttType = typeof(StatusInfoAttribute);

            foreach (var m in metadatas.OfType<StatusInfoMetadataAttribute>())
            {
                //Const fields like in ServerStatusInfoMetadata.cs
                foreach (var f in m.MetadataType.GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.IsLiteral))
                {
                    var i = f.GetCustomAttributes(statusInfoAttType, false).FirstOrDefault() as StatusInfoAttribute;

                    if (i == null)
                        continue;

                    i.Key = f.Name;
                    atts.Add(i);
                }

                foreach (var f in m.MetadataType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var i = f.GetCustomAttributes(statusInfoAttType, false).FirstOrDefault() as StatusInfoAttribute;

                    if (i == null)
                        continue;

                    i.Key = f.Name;
                    i.DataType = f.PropertyType;
                    atts.Add(i);
                }
            }

            return atts.OrderBy(a => a.Order).ToArray();
        }
    }
}
