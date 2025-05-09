using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A factory for creating instances of a specified pipeline filter type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    /// <typeparam name="TPipelineFilter">The type of the pipeline filter to create.</typeparam>
    public class DefaultPipelineFilterFactory<TPackageInfo, TPipelineFilter> : PipelineFilterFactoryBase<TPackageInfo>
        where TPipelineFilter : IPipelineFilter<TPackageInfo>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPipelineFilterFactory{TPackageInfo, TPipelineFilter}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public DefaultPipelineFilterFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new instance of the specified pipeline filter type.
        /// </summary>
        /// <returns>The created pipeline filter.</returns>
        protected override IPipelineFilter<TPackageInfo> Create()
        {
            return _serviceProvider.GetRequiredService<TPipelineFilter>();
        }
    }
}