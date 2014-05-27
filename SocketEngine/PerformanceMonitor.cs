using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class PerformanceMonitor : IPerformanceMonitor
    {
        public event Action<NodeStatus> OnStatusUpdate;

        private Timer m_PerformanceTimer;
        private int m_TimerInterval;
        private ILog m_PerfLog;

        private IWorkItem[] m_AppServers;

        private IWorkItem m_ServerManager;

        private ProcessPerformanceCounterHelper m_Helper;

        private List<KeyValuePair<string, StatusInfoAttribute[]>> m_ServerStatusMetadataSource;

        public PerformanceMonitor(IRootConfig config, IEnumerable<IWorkItem> appServers, IWorkItem serverManager, ILogFactory logFactory)
        {
            m_PerfLog = logFactory.GetLog("Performance");

            m_AppServers = appServers.ToArray();

            m_ServerManager = serverManager;

            m_Helper = new ProcessPerformanceCounterHelper(Process.GetCurrentProcess());

            m_TimerInterval = config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        }

        private void SetupServerStatusMetadata()
        {
            if (m_ServerStatusMetadataSource == null)
            {
                m_ServerStatusMetadataSource = new List<KeyValuePair<string, StatusInfoAttribute[]>>(m_AppServers.Length + 1);

                m_ServerStatusMetadataSource.Add(
                    new KeyValuePair<string, StatusInfoAttribute[]>(string.Empty,
                        new StatusInfoAttribute[]
                        {
                            new StatusInfoAttribute(StatusInfoKeys.CpuUsage) { Name = "CPU Usage", Format = "{0:0.00}%", Order = 0 },
                            new StatusInfoAttribute(StatusInfoKeys.MemoryUsage) { Name = "Physical Memory Usage", Format = "{0:N}", Order = 1 },
                            new StatusInfoAttribute(StatusInfoKeys.TotalThreadCount) { Name = "Total Thread Count", Order = 2 },
                            new StatusInfoAttribute(StatusInfoKeys.AvailableWorkingThreads) { Name = "Available Working Threads", Order = 3 },
                            new StatusInfoAttribute(StatusInfoKeys.AvailableCompletionPortThreads) { Name = "Available Completion Port Threads", Order = 4 },
                            new StatusInfoAttribute(StatusInfoKeys.MaxWorkingThreads) { Name = "Maximum Working Threads", Order = 5 },
                            new StatusInfoAttribute(StatusInfoKeys.MaxCompletionPortThreads) { Name = "Maximum Completion Port Threads", Order = 6 }
                        }));

                for (var i = 0; i < m_AppServers.Length; i++)
                {
                    var server = m_AppServers[i];
                    m_ServerStatusMetadataSource.Add(
                        new KeyValuePair<string, StatusInfoAttribute[]>(server.Name, server.GetServerStatusMetadata().OrderBy(s => s.Order).ToArray()));
                }

                if (m_ServerManager != null && m_ServerManager.State == ServerState.Running)
                {
                    m_ServerManager.TransferSystemMessage("ServerMetadataCollected", m_ServerStatusMetadataSource);
                }
            }
        }

        public void Start()
        {
            SetupServerStatusMetadata();
            m_PerformanceTimer.Change(0, m_TimerInterval);
        }

        public void Stop()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnPerformanceTimerCallback(object state)
        {
            var nodeStatus = new NodeStatus();

            StatusInfoCollection bootstrapStatus = new StatusInfoCollection();
            m_Helper.Collect(bootstrapStatus);

            nodeStatus.BootstrapStatus = bootstrapStatus;

            var instancesStatus = new List<StatusInfoCollection>(m_AppServers.Length);

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine("---------------------------------------------------");
            perfBuilder.AppendLine(string.Format("CPU Usage: {0:0.00}%, Physical Memory Usage: {1:N}, Total Thread Count: {2}", bootstrapStatus[StatusInfoKeys.CpuUsage], bootstrapStatus[StatusInfoKeys.MemoryUsage], bootstrapStatus[StatusInfoKeys.TotalThreadCount]));
            perfBuilder.AppendLine(string.Format("AvailableWorkingThreads: {0}, AvailableCompletionPortThreads: {1}", bootstrapStatus[StatusInfoKeys.AvailableWorkingThreads], bootstrapStatus[StatusInfoKeys.AvailableCompletionPortThreads]));
            perfBuilder.AppendLine(string.Format("MaxWorkingThreads: {0}, MaxCompletionPortThreads: {1}", bootstrapStatus[StatusInfoKeys.MaxWorkingThreads], bootstrapStatus[StatusInfoKeys.MaxCompletionPortThreads]));

            for (var i = 0; i < m_AppServers.Length; i++)
            {
                var s = m_AppServers[i];

                var metadata = m_ServerStatusMetadataSource[i + 1].Value;

                if (metadata == null)
                {
                    perfBuilder.AppendLine(string.Format("{0} ----------------------------------", s.Name));
                    perfBuilder.AppendLine(string.Format("{0}: {1}", "State", s.State));
                }
                else
                {
                    var serverStatus = s.CollectServerStatus(bootstrapStatus);

                    instancesStatus.Add(serverStatus);

                    perfBuilder.AppendLine(string.Format("{0} ----------------------------------", serverStatus.Tag));

                    for (var j = 0; j < metadata.Length; j++)
                    {
                        var statusInfoAtt = metadata[j];

                        if (!statusInfoAtt.OutputInPerfLog)
                            continue;

                        var statusValue = serverStatus[statusInfoAtt.Key];

                        if (statusValue == null)
                            continue;

                        perfBuilder.AppendLine(
                            string.Format("{0}: {1}", statusInfoAtt.Name,
                            string.IsNullOrEmpty(statusInfoAtt.Format) ? statusValue : string.Format(statusInfoAtt.Format, statusValue)));
                    }
                }
            }

            m_PerfLog.Info(perfBuilder.ToString());

            nodeStatus.InstancesStatus = instancesStatus.ToArray();

            if (OnStatusUpdate != null) OnStatusUpdate(nodeStatus);

            if (m_ServerManager != null && m_ServerManager.State == ServerState.Running)
            {
                m_ServerManager.TransferSystemMessage("ServerStatusCollected", nodeStatus);
            }
        }

        public void Dispose()
        {
            if (m_PerformanceTimer != null)
            {
                m_PerformanceTimer.Dispose();
                m_PerformanceTimer = null;
            }

            if (m_Helper != null)
            {
                m_Helper.Dispose();
                m_Helper = null;
            }
        }

        public int StatusUpdateInterval
        {
            get { return m_TimerInterval / 1000; }

            set
            {
                var newTimerInterval = value * 1000;

                if (m_TimerInterval == newTimerInterval)
                    return;

                m_TimerInterval = newTimerInterval;

                Stop();
                Start();
            }
        }
    }
}
