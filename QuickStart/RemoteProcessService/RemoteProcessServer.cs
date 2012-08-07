using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.RemoteProcessService
{
    public class RemoteProcessServer : AppServer<RemoteProcessSession>
    {
        private Dictionary<string, string> m_FrozedProcesses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private object m_FrozedProcessLock = new object();

        public string[] GetFrozedProcesses()
        {
            lock (m_FrozedProcessLock)
            {
                return m_FrozedProcesses.Values.ToArray();
            }
        }

        public void AddFrozedProcess(string processName)
        {
            lock(m_FrozedProcessLock)
            {
                m_FrozedProcesses[processName] = processName;
            }
        }

        public void RemoveFrozedProcess(string processName)
        {
            lock(m_FrozedProcessLock)
            {
                m_FrozedProcesses.Remove(processName);
            }
        }

        public void ClearFrozedProcess()
        {
            lock (m_FrozedProcessLock)
            {
                m_FrozedProcesses.Clear();
            }
        }

        public void StopFroze()
        {
            m_MonitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void StartFroze()
        {
            int interval = this.Config.Options.GetValue("MonitorInterval", "1").ToInt32();
            TimeSpan intervalTimeSpan = new TimeSpan(0, interval, 0);
            m_MonitorTimer.Change(intervalTimeSpan, intervalTimeSpan);
        }

        private Timer m_MonitorTimer;

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            int interval = config.Options.GetValue("MonitorInterval", "1").ToInt32();
            TimeSpan intervalTimeSpan = new TimeSpan(0, interval, 0);
            m_MonitorTimer = new Timer(new TimerCallback(OnMonitorTimerCallback), new object(), intervalTimeSpan, intervalTimeSpan);

            return true;
        }

        private void OnMonitorTimerCallback(object state)
        {
            if (Monitor.TryEnter(state))
            {
                try
                {
                    if (m_FrozedProcesses.Count <= 0)
                        return;

                    Process[] processes = Process.GetProcesses();

                    List<Process> toBeKilled = new List<Process>();

                    lock (m_FrozedProcessLock)
                    {
                        foreach (var p in processes)
                        {
                            if (m_FrozedProcesses.ContainsKey(p.ProcessName))
                                toBeKilled.Add(p);
                        }
                    }

                    foreach (var p in toBeKilled)
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch (Exception e)
                        {
                            if(Logger.IsErrorEnabled)
                                Logger.Error("Failed to kill the process " + p.ProcessName, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
        }

        public override void Stop()
        {
            m_MonitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_MonitorTimer.Dispose();
            base.Stop();
        }
    }
}
