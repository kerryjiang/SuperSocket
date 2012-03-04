using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public class PermformanceDataEventArgs : EventArgs
    {
        public GlobalPerformanceData GlobalData { get; private set; }

        public PerformanceDataInfo[] InstancesData { get; private set; }

        public PermformanceDataEventArgs(GlobalPerformanceData globalData, PerformanceDataInfo[] instancesData)
        {
            GlobalData = globalData;
            InstancesData = instancesData;
        }
    }
}
