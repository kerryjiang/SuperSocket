using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// PerformanceData class
    /// </summary>
    public class PerformanceData
    {
        /// <summary>
        /// Gets or sets the current record sample.
        /// </summary>
        /// <value>
        /// The current record.
        /// </value>
        public PerformanceRecord CurrentRecord { get; set; }

        /// <summary>
        /// Gets or sets the previous record sample.
        /// </summary>
        /// <value>
        /// The previous record.
        /// </value>
        public PerformanceRecord PreviousRecord { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceData"/> class.
        /// </summary>
        public PerformanceData()
        {
            CurrentRecord = new PerformanceRecord();
        }

        /// <summary>
        /// Pushes the latest record sample.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        public PerformanceData PushRecord(PerformanceRecord record)
        {
            PreviousRecord = CurrentRecord;
            CurrentRecord = record;
            CurrentRecord.RecordSpan = CurrentRecord.RecordTime.Subtract(PreviousRecord.RecordTime).TotalSeconds;
            return this;
        }
    }

    /// <summary>
    /// Performance record sample
    /// </summary>
    public class PerformanceRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceRecord"/> class.
        /// </summary>
        public PerformanceRecord()
        {
            RecordTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the total connections.
        /// </summary>
        /// <value>
        /// The total connections.
        /// </value>
        public int TotalConnections { get; set; }

        /// <summary>
        /// Gets or sets the total handled requests.
        /// </summary>
        /// <value>
        /// The total handled requests.
        /// </value>
        public long TotalHandledRequests { get; set; }

        /// <summary>
        /// Gets the record time.
        /// </summary>
        public DateTime RecordTime { get; private set; }

        /// <summary>
        /// Gets or sets the record span.
        /// </summary>
        /// <value>
        /// The record span.
        /// </value>
        public double RecordSpan { get; set; }
    }

    /// <summary>
    /// GlobalPerformanceData class
    /// </summary>
    public class GlobalPerformanceData
    {
        /// <summary>
        /// Gets or sets the available working threads.
        /// </summary>
        /// <value>
        /// The available working threads.
        /// </value>
        public int AvailableWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the available completion port threads.
        /// </summary>
        /// <value>
        /// The available completion port threads.
        /// </value>
        public int AvailableCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets or sets the max working threads.
        /// </summary>
        /// <value>
        /// The max working threads.
        /// </value>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets or sets the max completion port threads.
        /// </summary>
        /// <value>
        /// The max completion port threads.
        /// </value>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets or sets the total thread count.
        /// </summary>
        /// <value>
        /// The total thread count.
        /// </value>
        public int TotalThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the cpu usage.
        /// </summary>
        /// <value>
        /// The cpu usage.
        /// </value>
        public double CpuUsage { get; set; }

        /// <summary>
        /// Gets or sets the working set.
        /// </summary>
        /// <value>
        /// The working set.
        /// </value>
        public long WorkingSet { get; set; }
    }
}
