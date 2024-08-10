using System;
using System.Buffers;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    public class ProxyProtocolPipelineFilterFactory<TPackageInfo> : IPipelineFilterFactory<TPackageInfo>
    {
        private readonly IPipelineFilterFactory<TPackageInfo> _pipelineFilterFactory;

        public ProxyProtocolPipelineFilterFactory(IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
        {
            _pipelineFilterFactory = pipelineFilterFactory;
        }

        public IPipelineFilter<TPackageInfo> Create(object client)
        {
            return new ProxyProtocolPipelineFilter<TPackageInfo>(_pipelineFilterFactory.Create(client));
        }
    }
}