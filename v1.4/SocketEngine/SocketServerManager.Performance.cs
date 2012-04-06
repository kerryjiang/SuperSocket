using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    public static partial class SocketServerManager
    {
        private static Timer m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        private static int m_TimerInterval;

        private static PerformanceCounter m_CpuUsagePC;
        private static PerformanceCounter m_ThreadCountPC;
        private static PerformanceCounter m_WorkingSetPC;

        private static int m_CpuCores = 1;

        private static void OnPerformanceTimerCallback(object state)
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
                MaxWorkingThreads = maxWorkingThreads,
                MaxCompletionPortThreads = maxCompletionPortThreads,
                CpuUsage = m_CpuUsagePC.NextValue() / m_CpuCores,
                TotalThreadCount = (int)m_ThreadCountPC.NextValue(),
                WorkingSet = (long)m_WorkingSetPC.NextValue()
            };

            var perfBuilder = new StringBuilder();

            perfBuilder.AppendLine("---------------------------------------------------");
            perfBuilder.AppendLine(string.Format("CPU Usage: {0}%, Physical Memory Usage: {1:N}, Total Thread Count: {2}", globalPerfData.CpuUsage.ToString("0.00"), globalPerfData.WorkingSet, globalPerfData.TotalThreadCount));
            perfBuilder.AppendLine(string.Format("AvailableWorkingThreads: {0}, AvailableCompletionPortThreads: {1}", globalPerfData.AvailableWorkingThreads, globalPerfData.AvailableCompletionPortThreads));
            perfBuilder.AppendLine(string.Format("MaxWorkingThreads: {0}, MaxCompletionPortThreads: {1}", globalPerfData.MaxWorkingThreads, globalPerfData.MaxCompletionPortThreads));

            var instancesData = new List<PerformanceDataInfo>(m_ServerList.Count);

            m_ServerList.ForEach(s =>
                {
                    var perfSource = s as IPerformanceDataSource;
                    if (perfSource != null)
                    {
                        var perfData = perfSource.CollectPerformanceData(globalPerfData);

                        instancesData.Add(new PerformanceDataInfo { ServerName = s.Name, Data = perfData });

                        perfBuilder.AppendLine(string.Format("{0} - Total Connections: {1}, Total Handled Commands: {2}, Command Handling Speed: {3:f0}/s",
                            s.Name,
                            perfData.CurrentRecord.TotalConnections,
                            perfData.CurrentRecord.TotalHandledCommands,
                            (perfData.CurrentRecord.TotalHandledCommands - perfData.PreviousRecord.TotalHandledCommands) / perfData.CurrentRecord.RecordSpan));
                    }
                });

            LogUtil.LogPerf(perfBuilder.ToString());

            var handler = Messanger.GetHandler<PermformanceDataEventArgs>();

            if (handler != null)
            {
                handler(new PermformanceDataEventArgs(globalPerfData, instancesData.ToArray()));
            }
        }

        private static void StartPerformanceLog()
        {
            Process process = Process.GetCurrentProcess();

            m_CpuCores = Environment.ProcessorCount;

            m_CpuUsagePC = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            m_ThreadCountPC = new PerformanceCounter("Process", "Thread Count", process.ProcessName);
            m_WorkingSetPC = new PerformanceCounter("Process", "Working Set", process.ProcessName);

            m_TimerInterval = m_Config.PerformanceDataCollectInterval * 1000;

            m_PerformanceTimer.Change(0, m_TimerInterval);
        }

        private static void StopPerformanceLog()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);

            m_CpuUsagePC.Close();
            m_ThreadCountPC.Close();
            m_WorkingSetPC.Close();
        }
    }
}
