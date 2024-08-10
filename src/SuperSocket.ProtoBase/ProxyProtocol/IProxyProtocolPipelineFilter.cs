using System;
using System.Buffers;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    public interface IProxyProtocolPipelineFilter : IPipelineFilter
    {
        ProxyInfo ProxyInfo { get; }
    }
}