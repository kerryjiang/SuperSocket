using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Metadata;
using System.Diagnostics;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 112)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Memory Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 113)]
    partial class AppDomainAppServer
    {
        private static Process m_Process;

        private readonly static bool m_AppDomainMonitoringSupported = false;

        static AppDomainAppServer()
        {
            try
            {
                AppDomain.MonitoringIsEnabled = true;
                m_AppDomainMonitoringSupported = true;
            }
            catch (NotImplementedException)
            {
                return;
            }

            m_Process = Process.GetCurrentProcess();
        }

        protected override bool StatusMetadataExtended
        {
            get
            {
                return m_AppDomainMonitoringSupported;
            }
        }

        public override StatusInfoCollection CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            var statusCollection = base.CollectServerStatus(nodeStatus);

            if (!m_AppDomainMonitoringSupported)
                return statusCollection;

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
