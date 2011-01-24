using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine
{
    public static partial class SocketServerManager
    {
        private static Timer m_PerformanceTimer = new Timer(OnPerformanceTimerCallback);
        private static readonly int m_TimerInterval = 1000 * 60;//1 minute

        private static Timer m_CpuUsageTimer = new Timer(OnCpuUsageTimerCallback);
        private static readonly int m_CpuTimerInterval = 1000;//1 second
        private static double m_PrevTotalProcessorTime = 0;
        private static DateTime m_PrevCheckingTime = DateTime.MinValue;
        private static double m_CpuUsgae = 0;

        private static void OnPerformanceTimerCallback(object state)
        {
            var process = Process.GetCurrentProcess();
            LogUtil.LogPerf(string.Format("CPU Usage: {0}%, Physical Memory Usage: {1}, Virtual Memory Usage: {2}, Total Thread Count: {3}", m_CpuUsgae.ToString("0.00"), process.WorkingSet64, process.VirtualMemorySize64, process.Threads.Count));
            m_ServerList.ForEach(s => s.LogPerf());
        }

        private static void OnCpuUsageTimerCallback(object state)
        {
            var process = Process.GetCurrentProcess();

            if (m_PrevTotalProcessorTime == 0)
                m_PrevCheckingTime = process.StartTime;

            double currentProcessorTime = process.TotalProcessorTime.TotalMilliseconds;
            DateTime currentCheckingTime = DateTime.Now;

            m_CpuUsgae = (currentProcessorTime - m_PrevTotalProcessorTime) * 100 / currentCheckingTime.Subtract(m_PrevCheckingTime).TotalMilliseconds;
            m_PrevCheckingTime = currentCheckingTime;
            m_PrevTotalProcessorTime = currentProcessorTime;
        }

        private static void StartPerformanceLog()
        {
            m_PerformanceTimer.Change(m_TimerInterval, m_TimerInterval);
            m_CpuUsageTimer.Change(m_CpuTimerInterval, m_CpuTimerInterval);
        }

        private static void StopPerformanceLog()
        {
            m_PerformanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_CpuUsageTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
