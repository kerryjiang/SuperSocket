using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// PermformanceData event argument
    /// </summary>
    public class PermformanceDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the global data.
        /// </summary>
        public GlobalPerformanceData GlobalData { get; private set; }

        /// <summary>
        /// Gets all the instances performance data.
        /// </summary>
        public PerformanceDataInfo[] InstancesData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermformanceDataEventArgs"/> class.
        /// </summary>
        /// <param name="globalData">The global data.</param>
        /// <param name="instancesData">The instances data.</param>
        public PermformanceDataEventArgs(GlobalPerformanceData globalData, PerformanceDataInfo[] instancesData)
        {
            GlobalData = globalData;
            InstancesData = instancesData;
        }
    }
}
