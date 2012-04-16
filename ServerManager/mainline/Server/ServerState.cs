using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Server
{
    /// <summary>
    /// Server's state
    /// </summary>
    public class ServerState
    {
        /// <summary>
        /// Gets or sets the global performance.
        /// </summary>
        /// <value>
        /// The global performance.
        /// </value>
        public GlobalPerformanceData GlobalPerformance { get; set; }

        /// <summary>
        /// Gets or sets the instance states.
        /// </summary>
        /// <value>
        /// The instance states.
        /// </value>
        public InstanceState[] InstanceStates { get; set; }

        /// <summary>
        /// Convert to server's information which will be sent to client
        /// </summary>
        /// <returns></returns>
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
