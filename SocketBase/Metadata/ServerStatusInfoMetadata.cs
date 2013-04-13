using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// Server StatusInfo Metadata
    /// </summary>
    public class ServerStatusInfoMetadata
    {
        /// <summary>
        /// Gets or sets the started time.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        [StatusInfo("Started Time", Order = 0, DataType = typeof(DateTime?))]
        public const string StartedTime = "StartedTime";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        [StatusInfo("Is Running", Order = 1, DataType = typeof(bool))]
        public const string IsRunning = "IsRunning";

        /// <summary>
        /// Gets or sets the total count of the connections.
        /// </summary>
        /// <value>
        /// The total count of the connections.
        /// </value>
        [StatusInfo("Total Connections", Order = 2, DataType = typeof(int))]
        public const string TotalConnections = "TotalConnections";


        /// <summary>
        /// Gets or sets the maximum allowed connection number.
        /// </summary>
        /// <value>
        /// The max connection number.
        /// </value>
        [StatusInfo("Maximum Allowed Connection Number", ShortName = "Max Allowed Connections", Order = 3, DataType = typeof(int))]
        public const string MaxConnectionNumber = "MaxConnectionNumber";

        /// <summary>
        /// Gets or sets the total handled requests count.
        /// </summary>
        /// <value>
        /// The total handled requests count.
        /// </value>
        [StatusInfo("Total Handled Requests", Format = "{0:N0}", Order = 4, DataType = typeof(long))]
        public const string TotalHandledRequests = "TotalHandledRequests";

        /// <summary>
        /// Gets or sets the request handling speed, per second.
        /// </summary>
        /// <value>
        /// The request handling speed.
        /// </value>
        [StatusInfo("Request Handling Speed (#/second)", Format = "{0:f0}", Order = 5, DataType = typeof(double))]
        public const string RequestHandlingSpeed = "RequestHandlingSpeed";


        /// <summary>
        /// Gets or sets the listeners.
        /// </summary>
        [StatusInfo("Listeners", Order = 6, OutputInPerfLog = false, DataType = typeof(ListenerInfo[]))]
        public const string Listeners = "Listeners";


        /// <summary>
        /// Gets or sets the avialable sending queue items.
        /// </summary>
        /// <value>
        /// The avialable sending queue items.
        /// </value>
        [StatusInfo("Avialable Sending Queue Items", Format = "{0:N0}", Order = 7, DataType = typeof(int))]
        public const string AvialableSendingQueueItems = "AvialableSendingQueueItems";

        /// <summary>
        /// Gets or sets the total sending queue items.
        /// </summary>
        /// <value>
        /// The total sending queue items.
        /// </value>
        [StatusInfo("Total Sending Queue Items", Format = "{0:N0}", Order = 8, DataType = typeof(int))]
        public const string TotalSendingQueueItems = "TotalSendingQueueItems";
    }
}
