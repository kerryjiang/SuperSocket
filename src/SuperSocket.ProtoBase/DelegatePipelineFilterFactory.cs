using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter factory that uses a delegate to create pipeline filters.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class DelegatePipelineFilterFactory<TPackageInfo> : PipelineFilterFactoryBase<TPackageInfo>
    {
        private readonly Func<IPipelineFilter<TPackageInfo>> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePipelineFilterFactory{TPackageInfo}"/> class with the specified service provider and factory delegate.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="factory">The delegate used to create pipeline filters.</param>
        public DelegatePipelineFilterFactory(IServiceProvider serviceProvider, Func<IPipelineFilter<TPackageInfo>> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Creates a pipeline filter for the specified client using the factory delegate.
        /// </summary>
        /// <returns>The created pipeline filter.</returns>
        protected override IPipelineFilter<TPackageInfo> Create()
        {
            return _factory();
        }
    }
}