using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    class SuperSocketServerContainer : IServerContainer
    {
        private bool m_ServersLoaded = false;

        private List<IAppServer> m_Servers;

        public SuperSocketServerContainer(IRootConfig config)
        {
            m_TimerInterval = config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        }

        public IEnumerable<IAppServer> GetAllServers()
        {
            if (!m_ServersLoaded)
                throw new Exception("Servers have not been loaded");

            return m_Servers;
        }

        public IAppServer GetServerByName(string name)
        {
            if (!m_ServersLoaded)
                throw new Exception("Servers have not been loaded");

            for (int i = 0; i < m_Servers.Count; i++)
            {
                var server = m_Servers[i];

                if (server.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return server;
            }

            return null;
        }

        internal void LoadServers(List<IAppServer> servers)
        {
            m_Servers = servers;
            m_ServersLoaded = true;

            if (m_Loaded == null)
                return;

            m_Loaded(this, EventArgs.Empty);
        }

        private EventHandler<PermformanceDataEventArgs> m_PerformanceDataCollected;

        public event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected
        {
            add { m_PerformanceDataCollected += value; }
            remove { m_PerformanceDataCollected -= value; }
        }

        private EventHandler m_Loaded;

        public event EventHandler Loaded
        {
            add { m_Loaded += value; }
            remove { m_Loaded -= value; }
        }

        private Timer m_PerformanceTimer;
        private readonly int m_TimerInterval;
        private ILog m_PerfLog;

        private PerformanceCounter m_CpuUsagePC;
        private PerformanceCounter m_ThreadCountPC;
        private PerformanceCounter m_WorkingSetPC;

        private int m_CpuCores = 1;

        private void OnPerformanceTimerCallback(object state)
        {
            int availableWorkingThreads, availableCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out availableWorkingThreads, out availableCompletionPortThreads);

            int maxWorkingThreads;
            int maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThreads, out maxCompletionPortThreads);

            var globalPerfData = new GlobalPerformanceData
            {
                AvailableWorkingThreads = availableWorkingThreads,
                AvailableCompletionPortThreads = availableCompletionPortThreads,
                MaxCompletionPortThreads = maxCompletionPortThreads,
                MaxWorkingThreads = maxWorkingThreads,
                CpuUsage = m_CpuUsagePC.NextValue() / m_CpuCores,
                TotalThreadCount = (int)m_ThreadCountPC.NextValue(),
                WorkingSet = (long)m_WorkingSetPC.NextValue()
            };

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine("---------------------------------------------------");
            perfBuilder.AppendLine(string.Format("CPU Usage: {0}%, Physical Memory Usage: {1:N}, Total Thread Count: {2}", globalPerfData.CpuUsage.ToString("0.00"), globalPerfData.WorkingSet, globalPerfData.TotalThreadCount));
            perfBuilder.AppendLine(string.Format("AvailableWorkingThreads: {0}, AvailableCompletionPortThreads: {1}", globalPerfData.AvailableWorkingThreads, globalPerfData.AvailableCompletionPortThreads));
            perfBuilder.AppendLine(string.Format("MaxWorkingThreads: {0}, MaxCompletionPortThreads: {1}", globalPerfData.MaxWorkingThreads, globalPerfData.MaxCompletionPortThreads));

            var instancesData = new List<PerformanceDataInfo>(m_Servers.Count);

            m_Servers.ForEach(s =>
            {
                var perfSource = s as IPerformanceDataSource;
                if (perfSource != null)
                {
                    var perfData = perfSource.CollectPerformanceData(globalPerfData);

                    instancesData.Add(new PerformanceDataInfo { ServerName = s.Name, Data = perfData });

                    perfBuilder.AppendLine(string.Format("{0} - Total Connections: {1}, Total Handled Requests: {2}, Request Handling Speed: {3:f0}/s",
                        s.Name,
                        perfData.CurrentRecord.TotalConnections,
                        perfData.CurrentRecord.TotalHandledRequests,
                        (perfData.CurrentRecord.TotalHandledRequests - perfData.PreviousRecord.TotalHandledRequests) / perfData.CurrentRecord.RecordSpan));
                }
            });

            m_PerfLog.Info(perfBuilder.ToString());

            if (m_PerformanceDataCollected == null)
                return;

            m_PerformanceDataCollected.BeginInvoke(this, new PermformanceDataEventArgs(globalPerfData, instancesData.ToArray()), null, null);
        }

        internal void StartPerformanceLog()
        {
            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", process.ProcessName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", process.ProcessName);

            m_PerfLog = LogFactoryProvider.LogFactory.GetLog("performance");
            m_PerformanceTimer.Change(0, m_TimerInterval);
        }

        internal void StopPerformanceLog()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);

            m_CpuUsagePC.Close();
            m_ThreadCountPC.Close();
            m_WorkingSetPC.Close();
        }
    }
}
