using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    public class ServerInfo
    {
        public double CpuUsage { get; set; }

        public int AvailableWorkingThreads { get; set; }

        public int AvailableCompletionPortThreads { get; set; }

        public int MaxWorkingThreads { get; set; }

        public int MaxCompletionPortThreads { get; set; }

        public double PhysicalMemoryUsage { get; set; }

        public int TotalThreadCount { get; set; }

        public InstanceInfo[] Instances { get; set; }
    }
}
