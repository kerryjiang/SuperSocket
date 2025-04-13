using System;
using System.Buffers;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    /// <summary>
    /// Defines the functionality of a pipeline filter for processing proxy protocol packages.
    /// </summary>
    public interface IProxyProtocolPipelineFilter : IPipelineFilter
    {
        /// <summary>
        /// Gets the proxy information associated with the pipeline filter.
        /// </summary>
        ProxyInfo ProxyInfo { get; }
    }
}