using SuperSocket.SocketBase;
using System;
namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Interface of IPerformanceMonitor
    /// </summary>
    public interface IPerformanceMonitor : IDisposable
    {
        /// <summary>
        /// Start PerformanceMonitor.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop PerformanceMonitor.
        /// </summary>
        void Stop();
        /// <summary>
        /// Invoke when status update.
        /// </summary>
        event Action<NodeStatus> onUpdate;
        /// <summary>
        /// Get or Set the current refresh timer in seconds.
        /// </summary>
        int TimerInterval { get; set; }
    }
}
