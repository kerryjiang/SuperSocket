using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class DefaultPipelineFilterFactory<TPackageInfo, TPipelineFilter> : IPipelineFilterFactory<TPackageInfo>
        where TPackageInfo : class
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        public IPipelineFilter<TPackageInfo> Create(object client)
        {
            return new TPipelineFilter();
        }
    }
}