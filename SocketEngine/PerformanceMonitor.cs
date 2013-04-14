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
    class PerformanceMonitor : IDisposable
    {
        private Timer m_PerformanceTimer;
        private int m_TimerInterval;
        private ILog m_PerfLog;

        private PerformanceCounter m_CpuUsagePC;
        private PerformanceCounter m_ThreadCountPC;
        private PerformanceCounter m_WorkingSetPC;

        private int m_CpuCores = 1;

        private IWorkItem[] m_AppServers;

        private StatusInfoAttribute[][] m_ServerStatusMetadatas;

        private static readonly object[] m_ParaArray = new object[0];

        public PerformanceMonitor(IRootConfig config, IEnumerable<IWorkItem> appServers, ILogFactory logFactory)
        {
            m_PerfLog = logFactory.GetLog("Performance");

            m_AppServers = appServers.ToArray();

            SetupServerStatusMetadata();

            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            var instanceName = (isUnix || Platform.IsMono) ? string.Format("{0}/{1}", process.Id, process.ProcessName) : GetPerformanceCounterInstanceName(process);

            SetupPerformanceCounters(instanceName);

            m_TimerInterval = config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        }

        private void SetupServerStatusMetadata()
        {
            m_ServerStatusMetadatas = new StatusInfoAttribute[m_AppServers.Length][];

            for (var i = 0; i < m_AppServers.Length; i++)
            {
                m_ServerStatusMetadatas[i] = m_AppServers[i].GetServerStatusMetadata();
            }
        }

        private void SetupPerformanceCounters(string instanceName)
        {
            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", instanceName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", instanceName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", instanceName);
        }

        //Tt is only used in windows
        private static string GetPerformanceCounterInstanceName(Process process)
        {
            var processId = process.Id;
            var processCategory = new PerformanceCounterCategory("Process");
            var runnedInstances = processCategory.GetInstanceNames();

            foreach (string runnedInstance in runnedInstances)
            {
                if (!runnedInstance.StartsWith(process.ProcessName, StringComparison.OrdinalIgnoreCase))
                    continue;

                using (var performanceCounter = new PerformanceCounter("Process", "ID Process", runnedInstance, true))
                {
                    if ((int)performanceCounter.RawValue == processId)
                    {
                        return runnedInstance;
                    }
                }
            }

            return process.ProcessName;
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

            StatusInfoCollection globalPerfData = null;

            var retry = false;

            while(true)
            {
                try
                {
                    globalPerfData = new StatusInfoCollection();

                    globalPerfData[ProcessStatusInfoMetadata.AvailableWorkingThreads] = availableWorkingThreads;
                    globalPerfData[ProcessStatusInfoMetadata.AvailableCompletionPortThreads] = availableCompletionPortThreads;
                    globalPerfData[ProcessStatusInfoMetadata.MaxCompletionPortThreads] = maxCompletionPortThreads;
                    globalPerfData[ProcessStatusInfoMetadata.MaxWorkingThreads] = maxWorkingThreads;
                    globalPerfData[ProcessStatusInfoMetadata.TotalThreadCount] = (int)m_ThreadCountPC.NextValue();
                    globalPerfData[ProcessStatusInfoMetadata.CpuUsage] = m_CpuUsagePC.NextValue() / m_CpuCores;
                    globalPerfData[ProcessStatusInfoMetadata.WorkingSet] = (long)m_WorkingSetPC.NextValue();
                    break;
                }
                catch (InvalidOperationException e)
                {
                    //Only re-get performance counter one time
                    if (retry)
                        throw e;

                    //Only re-get performance counter for .NET/Windows
                    if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX || Platform.IsMono)
                        throw e;

                    //If a same name process exited, this process's performance counters instance name could be changed,
                    //so if the old performance counter cannot be access, get the performance counter's name again
                    var newInstanceName = GetPerformanceCounterInstanceName(Process.GetCurrentProcess());
                    SetupPerformanceCounters(newInstanceName);
                    retry = true;
                }
            }

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine("---------------------------------------------------");
            perfBuilder.AppendLine(string.Format("CPU Usage: {0:0.00}%, Physical Memory Usage: {1:N}, Total Thread Count: {2}", globalPerfData[ProcessStatusInfoMetadata.CpuUsage], globalPerfData[ProcessStatusInfoMetadata.WorkingSet], globalPerfData[ProcessStatusInfoMetadata.TotalThreadCount]));
            perfBuilder.AppendLine(string.Format("AvailableWorkingThreads: {0}, AvailableCompletionPortThreads: {1}", globalPerfData[ProcessStatusInfoMetadata.AvailableWorkingThreads], globalPerfData[ProcessStatusInfoMetadata.AvailableCompletionPortThreads]));
            perfBuilder.AppendLine(string.Format("MaxWorkingThreads: {0}, MaxCompletionPortThreads: {1}", globalPerfData[ProcessStatusInfoMetadata.MaxWorkingThreads], globalPerfData[ProcessStatusInfoMetadata.MaxCompletionPortThreads]));

            for (var i = 0; i < m_AppServers.Length; i++)
            {
                var s = m_AppServers[i];

                var serverStatus = s.CollectServerStatus(globalPerfData);
                var metadata = m_ServerStatusMetadatas[i];

                perfBuilder.AppendLine(string.Format("{0} ----------------------------------", serverStatus.Tag));

                for (var j = 0; j < metadata.Length; j++)
                {
                    var statusInfoAtt = metadata[j];

                    if (!statusInfoAtt.OutputInPerfLog)
                        continue;

                    perfBuilder.AppendLine(
                        string.Format("{0}: {1}", statusInfoAtt.Name,
                        string.IsNullOrEmpty(statusInfoAtt.Format) ? serverStatus[statusInfoAtt.Key] : string.Format(statusInfoAtt.Format, serverStatus[statusInfoAtt.Key])));
                }
            }

            m_PerfLog.Info(perfBuilder.ToString());
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
