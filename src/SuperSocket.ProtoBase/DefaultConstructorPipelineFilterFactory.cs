using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A factory for creating instances of a specified pipeline filter type which has default constructor.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    /// <typeparam name="TPipelineFilter">The type of the pipeline filter to create.</typeparam>
    public class DefaultConstructorPipelineFilterFactory<TPackageInfo, TPipelineFilter> : PipelineFilterFactoryBase<TPackageInfo>
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        /// <summary>
        /// Gets the package decoder used by the pipeline filters.
        /// </summary>
        protected IPackageDecoder<TPackageInfo> PackageDecoder { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConstructorPipelineFilterFactory{TPackageInfo, TPipelineFilter}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public DefaultConstructorPipelineFilterFactory(IServiceProvider serviceProvider)
        {
            PackageDecoder = serviceProvider.GetService(typeof(IPackageDecoder<TPackageInfo>)) as IPackageDecoder<TPackageInfo>;
        }

        /// <summary>
        /// Creates a new instance of the specified pipeline filter type.
        /// </summary>
        /// <returns>The created pipeline filter.</returns>
        protected override IPipelineFilter<TPackageInfo> Create()
        {
            var filter = new TPipelineFilter();
            filter.Decoder = PackageDecoder;
            return filter;
        }
    }
}