using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The status of one SuperSocket node (one installation or deployment)
    /// </summary>
    [Serializable]
    public class NodeStatus
    {
        /// <summary>
        /// Gets or sets the bootstrap status.
        /// </summary>
        /// <value>
        /// The bootstrap status.
        /// </value>
        public StatusInfoCollection BootstrapStatus { get; set; }

        /// <summary>
        /// Gets or sets the status of all server instances running in this node.
        /// </summary>
        /// <value>
        /// The instances status.
        /// </value>
        public StatusInfoCollection[] InstancesStatus { get; set; }

        /// <summary>
        /// Saves the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            var serializer = new BinaryFormatter();

            using (var stream = File.Create(filePath))
            {
                serializer.Serialize(stream, this);
                stream.Flush();
                stream.Close();
            }
        }

        /// <summary>
        /// Loads a NodeStatus instance from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static NodeStatus LoadFrom(string filePath)
        {
            var serializer = new BinaryFormatter();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                var status = serializer.Deserialize(stream) as NodeStatus;
                stream.Close();
                return status;
            }
        }
    }
}
