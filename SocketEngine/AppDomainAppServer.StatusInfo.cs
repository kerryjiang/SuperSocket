using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;
using System.Diagnostics;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 100)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Memory Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 101)]
    partial class AppDomainAppServer
    {
        private static Process m_Process;

        static AppDomainAppServer()
        {
            m_Process = Process.GetCurrentProcess();
            AppDomain.MonitoringIsEnabled = true;
        }

        public override StatusInfoCollection CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            var statusCollection = base.CollectServerStatus(nodeStatus);

            if (statusCollection != null && m_HostDomain != null)
            {
                if (m_Process.TotalProcessorTime.TotalMilliseconds > 0)
                {
                    var value = m_HostDomain.MonitoringTotalProcessorTime.TotalMilliseconds * 100 / m_Process.TotalProcessorTime.TotalMilliseconds;
                    statusCollection[StatusInfoKeys.CpuUsage] = value;
                }

                if (AppDomain.MonitoringSurvivedProcessMemorySize > 0)
                {
                    var value = (double)m_HostDomain.MonitoringSurvivedMemorySize * 100 / (double)AppDomain.MonitoringSurvivedProcessMemorySize;
                    statusCollection[StatusInfoKeys.MemoryUsage] = (double)value;
                }
            }

            return statusCollection;
        }
    }
}
