using System;

namespace SuperSocket.ProtoBase
{
    public abstract class PipelineFilterFactoryBase<TPackageInfo> : IPipelineFilterFactory<TPackageInfo>
    {
        protected IPackageDecoder<TPackageInfo> PackageDecoder { get; private set; }
        
        public PipelineFilterFactoryBase(IServiceProvider serviceProvider)
        {
            PackageDecoder = serviceProvider.GetService(typeof(IPackageDecoder<TPackageInfo>)) as IPackageDecoder<TPackageInfo>;
        }

        protected abstract IPipelineFilter<TPackageInfo> CreateCore(object client);

        public virtual IPipelineFilter<TPackageInfo> Create(object client)
        {
            var filter = CreateCore(client);
            filter.Decoder = PackageDecoder;
            return filter;
        }
    }
}