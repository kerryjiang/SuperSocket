using System;

namespace SuperSocket.ProtoBase
{
    public class DelegatePipelineFilterFactory<TPackageInfo> : IPipelineFilterFactory<TPackageInfo>
        where TPackageInfo : class
    {
        private Func<object, IPipelineFilter<TPackageInfo>> _factory;

        public DelegatePipelineFilterFactory(Func<object, IPipelineFilter<TPackageInfo>> factory)
        {
            _factory = factory;
        }

        public IPipelineFilter<TPackageInfo> Create(object client)
        {
            return _factory(client);
        }
    }
}