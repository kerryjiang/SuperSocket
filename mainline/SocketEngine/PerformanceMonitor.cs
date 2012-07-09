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
    class PerformanceMonitor : IDisposable
    {
        private Timer m_PerformanceTimer;
        private int m_TimerInterval;
        private ILog m_PerfLog;

        private PerformanceCounter m_CpuUsagePC;
        private PerformanceCounter m_ThreadCountPC;
        private PerformanceCounter m_WorkingSetPC;

        private int m_CpuCores = 1;

        private EventHandler<PermformanceDataEventArgs> m_Collected;

        private IWorkItem[] m_AppServers;

        public PerformanceMonitor(IRootConfig config, IEnumerable<IWorkItem> appServers, ILogFactory logFactory)
        {
            m_AppServers = appServers.ToArray();

            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", process.ProcessName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", process.ProcessName);

            m_PerfLog = logFactory.GetLog("performance");

            m_TimerInterval = config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        }

        /// <summary>
        /// Occurs when [performance data collected].
        /// </summary>
        public event EventHandler<PermformanceDataEventArgs> Collected
        {
            add { m_Collected += value; }
            remove { m_Collected -= value; }
        }

        public void Start()
        {
            m_PerformanceTimer.Change(0, m_TimerInterval);
        }

        public void Stop()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

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

            var instancesData = new PerformanceDataInfo[m_AppServers.Length];

            for (var i = 0; i < m_AppServers.Length; i++)
            {
                var s = m_AppServers[i];

                var perfSource = s as IPerformanceDataSource;
                if (perfSource != null)
                {
                    var perfData = perfSource.CollectPerformanceData(globalPerfData);

                    instancesData[i] = new PerformanceDataInfo { ServerName = s.Name, Data = perfData };

                    perfBuilder.AppendLine(string.Format("{0} - Total Connections: {1}, Total Handled Requests: {2}, Request Handling Speed: {3:f0}/s",
                        s.Name,
                        perfData.CurrentRecord.TotalConnections,
                        perfData.CurrentRecord.TotalHandledRequests,
                        (perfData.CurrentRecord.TotalHandledRequests - perfData.PreviousRecord.TotalHandledRequests) / perfData.CurrentRecord.RecordSpan));
                }
            }

            m_PerfLog.Info(perfBuilder.ToString());

            var handler = m_Collected;
            if (handler == null)
                return;

            handler.BeginInvoke(this, new PermformanceDataEventArgs(globalPerfData, instancesData), null, null);
        }

        public void Dispose()
        {
            if (m_PerformanceTimer != null)
            {
                m_PerformanceTimer.Dispose();
                m_PerformanceTimer = null;
            }

            if (m_CpuUsagePC != null)
            {
                m_CpuUsagePC.Close();
                m_CpuUsagePC = null;
            }

            if (m_ThreadCountPC != null)
            {
                m_ThreadCountPC.Close();
                m_ThreadCountPC = null;
            }

            if (m_WorkingSetPC != null)
            {
                m_WorkingSetPC.Close();
                m_WorkingSetPC = null;
            }
        }
    }
}
