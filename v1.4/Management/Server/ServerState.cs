using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.Management.Shared;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Management.Server
{
    public class ServerState
    {
        public GlobalPerformanceData GlobalPerformance { get; set; }

        public InstanceState[] InstanceStates { get; set; }

        public ServerInfo ToServerInfo()
        {
            return new ServerInfo
            {
                AvailableCompletionPortThreads = GlobalPerformance.AvailableCompletionPortThreads,
                AvailableWorkingThreads = GlobalPerformance.AvailableWorkingThreads,
                MaxWorkingThreads = GlobalPerformance.MaxWorkingThreads,
                MaxCompletionPortThreads = GlobalPerformance.MaxCompletionPortThreads,
                CpuUsage = GlobalPerformance.CpuUsage,
                PhysicalMemoryUsage = GlobalPerformance.WorkingSet,
                TotalThreadCount = GlobalPerformance.TotalThreadCount,
                Instances = InstanceStates.Select(i => new InstanceInfo
                {
                    CurrentConnectionCount = i.Performance.CurrentRecord.TotalConnections,
                    Listener = GetListenerDescription(i.Instance.Config),
                    Name = i.Instance.Name,
                    IsRunning = i.Instance.IsRunning,
                    MaxConnectionCount = i.Instance.Config.MaxConnectionNumber,
                    StartedTime = i.Instance.StartedTime,
                    RequestHandlingSpeed = (int)((i.Performance.CurrentRecord.TotalHandledCommands - i.Performance.PreviousRecord.TotalHandledCommands) / i.Performance.CurrentRecord.RecordSpan)
                }).ToArray()
            };
        }

        private string GetListenerDescription(IServerConfig config)
        {
            return (string.IsNullOrEmpty(config.Security) || config.Security.Equals("None"))
                ? string.Format("{0}:{1}", config.Ip, config.Port) : string.Format("[{0}] {1}:{2}", config.Security, config.Ip, config.Port);
        }
    }
}
