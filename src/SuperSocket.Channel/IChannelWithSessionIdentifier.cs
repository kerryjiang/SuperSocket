using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IChannelWithSessionIdentifier
    {
        string SessionIdentifier { get; }
    }
}
