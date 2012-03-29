using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using System.Threading;
using SuperSocket.Common.Logging;
using System.Diagnostics;
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
            m_CpuUsageTimer = new Timer(OnCpuUsageTimerCallback);
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
        private Timer m_CpuUsageTimer;
        private readonly int m_TimerInterval;
        private readonly int m_CpuTimerInterval = 1000;//1 second
        private double m_PrevTotalProcessorTime = 0;
        private DateTime m_PrevCheckingTime = DateTime.MinValue;
        private double m_CpuUsgae = 0;
        private readonly long m_MbUnit = 1024 * 1024;
        private ILog m_PerfLog;

        private void OnPerformanceTimerCallback(object state)
        {
            var process = Process.GetCurrentProcess();

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
                CpuUsage = m_CpuUsgae,
                TotalThreadCount = process.Threads.Count,
                WorkingSet = process.WorkingSet64,
                VirtualMemorySize = process.VirtualMemorySize64
            };

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine(string.Format("CPU Usage: {0}%, Physical Memory Usage: {1}M, Virtual Memory Usage: {2}M, Total Thread Count: {3}", globalPerfData.CpuUsage.ToString("0.00"), globalPerfData.WorkingSet / m_MbUnit, globalPerfData.VirtualMemorySize / m_MbUnit, globalPerfData.TotalThreadCount));
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

                    perfBuilder.AppendLine(string.Format("{0} - Total connections: {1}, total handled requests: {2}, request handling speed: {3}/s",
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

        private void OnCpuUsageTimerCallback(object state)
        {
            var process = Process.GetCurrentProcess();

            if (m_PrevTotalProcessorTime == 0)
                m_PrevCheckingTime = process.StartTime;

            double currentProcessorTime = process.TotalProcessorTime.TotalMilliseconds;
            DateTime currentCheckingTime = DateTime.Now;

            m_CpuUsgae = (currentProcessorTime - m_PrevTotalProcessorTime) * 100 / (currentCheckingTime.Subtract(m_PrevCheckingTime).TotalMilliseconds * Environment.ProcessorCount);
            m_PrevCheckingTime = currentCheckingTime;
            m_PrevTotalProcessorTime = currentProcessorTime;
        }

        internal void StartPerformanceLog()
        {
            m_PerfLog = LogFactoryProvider.LogFactory.GetLog("performance");
            m_PerformanceTimer.Change(0, m_TimerInterval);
            m_CpuUsageTimer.Change(m_CpuTimerInterval, m_CpuTimerInterval);
        }

        internal void StopPerformanceLog()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_CpuUsageTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
