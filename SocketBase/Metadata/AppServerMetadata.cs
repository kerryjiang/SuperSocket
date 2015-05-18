using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// AppServer's default metadata type
    /// </summary>
    [StatusInfo(StatusInfoKeys.StartedTime, Name = "Started Time", DataType = typeof(DateTime), Order = 0)]
    [StatusInfo(StatusInfoKeys.IsRunning, Name = "Is Running", DataType = typeof(bool), Order = 100)]
    [StatusInfo(StatusInfoKeys.TotalConnections, Name = "Total Connections", DataType = typeof(int), Order = 200)]
    [StatusInfo(StatusInfoKeys.MaxConnectionNumber, Name = "Maximum Allowed Connection Number", ShortName = "Max Allowed Connections", DataType = typeof(int), Order = 300)]
    [StatusInfo(StatusInfoKeys.TotalHandledRequests, Name = "Total Handled Requests", Format = "{0:N0}", DataType = typeof(long), Order = 400)]
    [StatusInfo(StatusInfoKeys.RequestHandlingSpeed, Name = "Request Handling Speed (#/second)", Format = "{0:f0}", DataType = typeof(double), Order = 500)]
    [StatusInfo(StatusInfoKeys.Listeners, Name = "Listeners", DataType = typeof(string), OutputInPerfLog = false, Order = 600)]
    [StatusInfo(StatusInfoKeys.AvialableSendingQueueItems, Name = "Avialable Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 700)]
    [StatusInfo(StatusInfoKeys.TotalSendingQueueItems, Name = "Total Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 800)]
    [Serializable]
    public class AppServerMetadata
    {
        /// <summary>
        /// Gets/sets the status fields.
        /// </summary>
        /// <value>
        /// The status fields.
        /// </value>
        public StatusInfoAttribute[] StatusFields { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is server manager.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is server manager; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsServerManager
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServerMetadata"/> class.
        /// </summary>
        public AppServerMetadata()
        {
            StatusFields = this.GetType()
                .GetCustomAttributes(typeof(StatusInfoAttribute), true)
                .OfType<StatusInfoAttribute>().ToArray();
        }



        /// <summary>
        /// Gets the status info metadata from the server type.
        /// </summary>
        /// <param name="serverType">Type of the server.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static AppServerMetadata GetAppServerMetadata(Type serverType)
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
                    return Activator.CreateInstance(serverMetadataTypeAtt.MetadataType) as AppServerMetadata;
                }

                if (serverType.BaseType == null)
                    return null;

                serverType = serverType.BaseType;
            }
        }
    }
}
