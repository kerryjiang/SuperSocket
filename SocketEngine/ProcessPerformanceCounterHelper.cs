using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class ProcessPerformanceCounterHelper : IDisposable
    {
        private PerformanceCounter m_CpuUsagePC;
        private PerformanceCounter m_ThreadCountPC;
        private PerformanceCounter m_WorkingSetPC;
        private int m_CpuCores = 1;

        private Process m_Process;

        public ProcessPerformanceCounterHelper(Process process)
        {
            m_Process = process;
            m_CpuCores = Environment.ProcessorCount;

            //Windows .Net, to avoid same name process issue
            if (!Platform.IsMono)
                RegisterSameNameProcesses(process);

            SetupPerformanceCounters();
        }

        private void RegisterSameNameProcesses(Process process)
        {
            foreach (var p in Process.GetProcessesByName(process.ProcessName).Where(x => x.Id != process.Id))
            {
                p.EnableRaisingEvents = true;
                p.Exited += new EventHandler(SameNameProcess_Exited);
            }
        }

        //When find a same name process exit, re-initialize the performance counters
        //because the performance counters' instance names could have been changed
        void SameNameProcess_Exited(object sender, EventArgs e)
        {
            SetupPerformanceCounters();
        }

        private void SetupPerformanceCounters()
        {
            var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            var instanceName = (isUnix || Platform.IsMono) ? string.Format("{0}/{1}", m_Process.Id, m_Process.ProcessName) : GetPerformanceCounterInstanceName(m_Process);

            SetupPerformanceCounters(instanceName);
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

        public void Collect(StatusInfoCollection statusCollection)
        {
            int availableWorkingThreads, availableCompletionPortThreads;
            ThreadPool.GetAvailableThreads(out availableWorkingThreads, out availableCompletionPortThreads);

            int maxWorkingThreads;
            int maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThreads, out maxCompletionPortThreads);

            var retry = false;

            while (true)
            {
                try
                {
                    statusCollection[StatusInfoKeys.AvailableWorkingThreads] = availableWorkingThreads;
                    statusCollection[StatusInfoKeys.AvailableCompletionPortThreads] = availableCompletionPortThreads;
                    statusCollection[StatusInfoKeys.MaxCompletionPortThreads] = maxCompletionPortThreads;
                    statusCollection[StatusInfoKeys.MaxWorkingThreads] = maxWorkingThreads;
                    statusCollection[StatusInfoKeys.TotalThreadCount] = (int)m_ThreadCountPC.NextValue();
                    statusCollection[StatusInfoKeys.CpuUsage] = m_CpuUsagePC.NextValue() / m_CpuCores;
                    statusCollection[StatusInfoKeys.MemoryUsage] = (long)m_WorkingSetPC.NextValue();

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
                    var newInstanceName = GetPerformanceCounterInstanceName(m_Process);
                    SetupPerformanceCounters(newInstanceName);
                    retry = true;
                }
            }
        }

        public void Dispose()
        {
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
