using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Provides a base class for creating pipeline filter factories.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class PipelineFilterFactoryBase<TPackageInfo> : IPipelineFilterFactory<TPackageInfo>, IPipelineFilterFactory
    {
        /// <summary>
        /// Gets the package decoder used by the pipeline filters.
        /// </summary>
        protected IPackageDecoder<TPackageInfo> PackageDecoder { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineFilterFactoryBase{TPackageInfo}"/> class with the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        public PipelineFilterFactoryBase(IServiceProvider serviceProvider)
        {
            PackageDecoder = serviceProvider.GetService(typeof(IPackageDecoder<TPackageInfo>)) as IPackageDecoder<TPackageInfo>;
        }

        /// <summary>
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter.</returns>
        protected abstract IPipelineFilter<TPackageInfo> CreateCore(object client);

        /// <summary>
        /// Creates a pipeline filter for the specified client and assigns the package decoder.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter with the package decoder assigned.</returns>
        public virtual IPipelineFilter<TPackageInfo> Create(object client)
        {
            var filter = CreateCore(client);
            filter.Decoder = PackageDecoder;
            return filter;
        }

        /// <summary>
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter.</returns>
        IPipelineFilter IPipelineFilterFactory.Create(object client)
        {
            return this.Create(client) as IPipelineFilter;
        }
    }
}