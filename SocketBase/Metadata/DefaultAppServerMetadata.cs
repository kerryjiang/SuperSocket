using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Metadata
{
    /// <summary>
    /// AppServer's default metadata type
    /// </summary>
    [StatusInfo(StatusInfoKeys.StartedTime, Name = "Started Time", DataType = typeof(DateTime), Order = 0)]
    [StatusInfo(StatusInfoKeys.IsRunning, Name = "Is Running", DataType = typeof(bool), Order = 100)]
    [StatusInfo(StatusInfoKeys.TotalConnections, Name = "Total Connections", DataType = typeof(int), Order = 200)]
    [StatusInfo(StatusInfoKeys.MaxConnectionNumber, Name = "Maximum Allowed Connection Number", ShortName = "Max Allowed Connections", DataType = typeof(int), Order = 300)]
    [StatusInfo(StatusInfoKeys.TotalHandledRequests, Name = "Total Handled Requests", Format = "{0:N0}", DataType = typeof(long), Order = 400)]
    [StatusInfo(StatusInfoKeys.RequestHandlingSpeed, Name = "Request Handling Speed (#/second)", Format = "{0:f0}", DataType = typeof(double), Order = 500)]
    [StatusInfo(StatusInfoKeys.Listeners, Name = "Listeners", DataType = typeof(string), OutputInPerfLog = false, Order = 600)]
    [StatusInfo(StatusInfoKeys.AvialableSendingQueueItems, Name = "Avialable Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 700)]
    [StatusInfo(StatusInfoKeys.TotalSendingQueueItems, Name = "Total Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 800)]
    public class DefaultAppServerMetadata
    {

    }
}
