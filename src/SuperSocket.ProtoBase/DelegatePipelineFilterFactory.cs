using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter factory that uses a delegate to create pipeline filters.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class DelegatePipelineFilterFactory<TPackageInfo> : PipelineFilterFactoryBase<TPackageInfo>
    {
        private readonly Func<object, IPipelineFilter<TPackageInfo>> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePipelineFilterFactory{TPackageInfo}"/> class with the specified service provider and factory delegate.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="factory">The delegate used to create pipeline filters.</param>
        public DelegatePipelineFilterFactory(IServiceProvider serviceProvider, Func<object, IPipelineFilter<TPackageInfo>> factory)
            : base(serviceProvider)
        {
            _factory = factory;
        }

        /// <summary>
        /// Creates a pipeline filter for the specified client using the factory delegate.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter.</returns>
        protected override IPipelineFilter<TPackageInfo> CreateCore(object client)
        {
            return _factory(client);
        }
    }
}