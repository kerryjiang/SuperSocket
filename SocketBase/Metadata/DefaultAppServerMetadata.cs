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
    [StatusInfo(StatusInfoKeys.IsRunning, Name = "Is Running", DataType = typeof(bool), Order = 1)]
    [StatusInfo(StatusInfoKeys.TotalConnections, Name = "TotalConnections", DataType = typeof(int), Order = 2)]
    [StatusInfo(StatusInfoKeys.MaxConnectionNumber, Name = "Maximum Allowed Connection Number", ShortName = "Max Allowed Connections", DataType = typeof(int), Order = 3)]
    [StatusInfo(StatusInfoKeys.TotalHandledRequests, Name = "Total Handled Requests", Format = "{0:N0}", DataType = typeof(long), Order = 4)]
    [StatusInfo(StatusInfoKeys.RequestHandlingSpeed, Name = "Request Handling Speed (#/second)", Format = "{0:f0}", DataType = typeof(double), Order = 5)]
    [StatusInfo(StatusInfoKeys.Listeners, Name = "Listeners", DataType = typeof(string), OutputInPerfLog = false, Order = 6)]
    [StatusInfo(StatusInfoKeys.AvialableSendingQueueItems, Name = "Avialable Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 7)]
    [StatusInfo(StatusInfoKeys.TotalSendingQueueItems, Name = "Total Sending Queue Items", DataType = typeof(int), Format = "{0:N0}", Order = 8)]
    public class DefaultAppServerMetadata
    {

    }
}
