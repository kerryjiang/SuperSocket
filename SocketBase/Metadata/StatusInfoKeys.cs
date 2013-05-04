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
    public class StatusInfoKeys
    {
        #region For server instance

        /// <summary>
        /// The started time.
        /// </summary>
        public const string StartedTime = "StartedTime";


        /// <summary>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </summary>
        public const string IsRunning = "IsRunning";

        /// <summary>
        /// The total count of the connections.
        /// </summary>
        public const string TotalConnections = "TotalConnections";

        /// <summary>
        /// The max connection number.
        /// </summary>
        public const string MaxConnectionNumber = "MaxConnectionNumber";

        /// <summary>
        /// The total handled requests count.
        /// </summary>
        public const string TotalHandledRequests = "TotalHandledRequests";

        /// <summary>
        /// Gets or sets the request handling speed, per second.
        /// </summary>
        /// <value>
        /// The request handling speed.
        /// </value>
        public const string RequestHandlingSpeed = "RequestHandlingSpeed";


        /// <summary>
        /// Gets or sets the listeners.
        /// </summary>
        public const string Listeners = "Listeners";

        /// <summary>
        /// The avialable sending queue items.
        /// </summary>
        public const string AvialableSendingQueueItems = "AvialableSendingQueueItems";

        /// <summary>
        /// The total sending queue items.
        /// </summary>
        public const string TotalSendingQueueItems = "TotalSendingQueueItems";

        #endregion

        #region For global service

        /// <summary>
        /// The cpu usage.
        /// </summary>
        public const string CpuUsage = "CpuUsage";

        /// <summary>
        /// The working set.
        /// </summary>
        public const string WorkingSet = "WorkingSet";

        /// <summary>
        /// The total thread count.
        /// </summary>
        public const string TotalThreadCount = "TotalThreadCount";

        /// <summary>
        /// The available working threads.
        /// </summary>
        public const string AvailableWorkingThreads = "AvailableWorkingThreads";

        /// <summary>
        /// The available completion port threads.
        /// </summary>
        public const string AvailableCompletionPortThreads = "AvailableCompletionPortThreads";

        /// <summary>
        /// The max working threads.
        /// </summary>
        public const string MaxWorkingThreads = "MaxWorkingThreads";

        /// <summary>
        /// The max completion port threads.
        /// </summary>
        public const string MaxCompletionPortThreads = "MaxCompletionPortThreads";

        #endregion

    }
}
