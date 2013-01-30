using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Server State
    /// </summary>
    [Serializable]
    public class ServerSummary
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the collected time.
        /// </summary>
        /// <value>
        /// The collected time.
        /// </value>
        public DateTime CollectedTime { get; set; }

        /// <summary>
        /// Gets or sets the started time.
        /// </summary>
        /// <value>
        /// The started time.
        /// </value>
        [Display("Started Time", Order = 0)]
        public DateTime? StartedTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        [Display("Is Running", Order = 1)]
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the total count of the connections.
        /// </summary>
        /// <value>
        /// The total count of the connections.
        /// </value>
        [Display("Total Connections", Order = 2)]
        public int TotalConnections { get; set; }


        /// <summary>
        /// Gets or sets the maximum allowed connection number.
        /// </summary>
        /// <value>
        /// The max connection number.
        /// </value>
        [Display("Maximum Allowed Connection Number", ShortName = "Max Allowed Connections", Order = 3)]
        public int MaxConnectionNumber { get; set; }

        /// <summary>
        /// Gets or sets the total handled requests count.
        /// </summary>
        /// <value>
        /// The total handled requests count.
        /// </value>
        [Display("Total Handled Requests", Format = "{0:N0}", Order = 4)]
        public long TotalHandledRequests { get; set; }

        /// <summary>
        /// Gets or sets the request handling speed, per second.
        /// </summary>
        /// <value>
        /// The request handling speed.
        /// </value>
        [Display("Request Handling Speed (#/second)", Format = "{0:f0}", Order = 5)]
        public double RequestHandlingSpeed { get; set; }


        /// <summary>
        /// Gets or sets the listeners.
        /// </summary>
        /// <value>
        /// The listeners.
        /// </value>
        [Display("Listeners", Order = 6, OutputInPerfLog = false)]
        public ListenerInfo[] Listeners { get; set; }


        /// <summary>
        /// Gets or sets the avialable sending queue items.
        /// </summary>
        /// <value>
        /// The avialable sending queue items.
        /// </value>
        [Display("Avialable Sending Queue Items", Format = "{0:N0}", Order = 7)]
        public int AvialableSendingQueueItems { get; set; }

        /// <summary>
        /// Gets or sets the total sending queue items.
        /// </summary>
        /// <value>
        /// The total sending queue items.
        /// </value>
        [Display("Total Sending Queue Items", Format = "{0:N0}", Order = 8)]
        public int TotalSendingQueueItems { get; set; }
    }
}
