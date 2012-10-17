using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using System.Reflection;

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

        private Dictionary<Type, List<KeyValuePair<PropertyInfo, DisplayAttribute>>> m_StateMetadataDict = new Dictionary<Type, List<KeyValuePair<PropertyInfo, DisplayAttribute>>>();

        private static readonly object[] m_ParaArray = new object[0];

        public PerformanceMonitor(IRootConfig config, IEnumerable<IWorkItem> appServers, ILogFactory logFactory)
        {
            m_AppServers = appServers.ToArray();

            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            var instanceName = isUnix ? string.Format("{0}/{1}", process.Id, process.ProcessName) : GetPerformanceCounterInstanceName(process);

            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", instanceName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", instanceName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", instanceName);

            m_PerfLog = logFactory.GetLog("Performance");

            m_TimerInterval = config.PerformanceDataCollectInterval * 1000;
            m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        }

        //Tt is only used in windows
        private static string GetPerformanceCounterInstanceName(Process process)
        {
            var processId = process.Id;
            var processCategory = new PerformanceCounterCategory("Process");
            var runnedInstances = processCategory.GetInstanceNames();

            foreach (string runnedInstance in runnedInstances)
            {
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

        private List<KeyValuePair<PropertyInfo, DisplayAttribute>> GetStateTypeMetatdata(Type stateType)
        {
            List<KeyValuePair<PropertyInfo, DisplayAttribute>> stateMetadata;

            if (m_StateMetadataDict.TryGetValue(stateType, out stateMetadata))
                return stateMetadata;

            stateMetadata = new List<KeyValuePair<PropertyInfo, DisplayAttribute>>();

            foreach (var p in stateType.GetProperties())
            {
                var att = p.GetCustomAttributes(false).FirstOrDefault() as DisplayAttribute;

                if (att != null && att.OutputInPerfLog)
                {
                    stateMetadata.Add(new KeyValuePair<PropertyInfo, DisplayAttribute>(p, att));
                }
            }

            m_StateMetadataDict.Add(stateType, stateMetadata.OrderBy(s => s.Value.Order).ToList());
            return stateMetadata;
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

            for (var i = 0; i < m_AppServers.Length; i++)
            {
                var s = m_AppServers[i];

                var serverStateSource = s as IServerStateSource;
                if (serverStateSource != null)
                {
                    var serverState = serverStateSource.CollectServerState(globalPerfData);
                    var stateTypeMetadata = GetStateTypeMetatdata(serverState.GetType());

                    perfBuilder.AppendLine(string.Format("{0} ----------------------------------", s.Name));

                    for (var j = 0; j < stateTypeMetadata.Count; j++)
                    {
                        var property = stateTypeMetadata[j];
                        if (!string.IsNullOrEmpty(property.Value.Format))
                            perfBuilder.AppendLine(string.Format("{0}: {1}", property.Value.Name, string.Format(property.Value.Format, property.Key.GetValue(serverState, m_ParaArray))));
                        else
                            perfBuilder.AppendLine(string.Format("{0}: {1}", property.Value.Name, property.Key.GetValue(serverState, m_ParaArray)));
                    }
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
