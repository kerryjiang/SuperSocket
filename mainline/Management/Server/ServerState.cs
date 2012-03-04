using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.Management.Shared;

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
                VirtualMemoryUsage = GlobalPerformance.VirtualMemorySize,
                TotalThreadCount = GlobalPerformance.TotalThreadCount,
                Instances = InstanceStates.Select(i => new InstanceInfo
                {
                    CurrentConnectionCount = i.Performance.CurrentRecord.TotalConnections,
                    Listeners = i.Instance.Listeners.Select(l => new SuperSocket.Management.Shared.ListenerInfo
                    {
                        BackLog = l.BackLog,
                        EndPoint = l.EndPoint.ToString(),
                         Security = l.Security.ToString()
                    }).ToArray(),
                    Name = i.Instance.Name,
                    IsRunning = i.Instance.IsRunning,
                    MaxConnectionCount = i.Instance.Config.MaxConnectionNumber,
                    StartedTime = i.Instance.StartedTime,
                    RequestHandlingSpeed = (int)((i.Performance.CurrentRecord.TotalHandledRequests - i.Performance.PreviousRecord.TotalHandledRequests) / i.Performance.CurrentRecord.RecordSpan)
                }).ToArray()
            };
        }
    }
}
