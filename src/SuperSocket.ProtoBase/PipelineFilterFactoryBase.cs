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
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <returns>The created pipeline filter.</returns>
        protected abstract IPipelineFilter<TPackageInfo> Create();

        /// <summary>
        /// Creates a pipeline filter for the specified client and assigns the package decoder.
        /// </summary>
        /// <returns>The created pipeline filter with the package decoder assigned.</returns>
        IPipelineFilter<TPackageInfo> IPipelineFilterFactory<TPackageInfo>.Create()
        {
            return Create();
        }

        /// <summary>
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <returns>The created pipeline filter.</returns>
        IPipelineFilter IPipelineFilterFactory.Create()
        {
            return Create();
        }
    }
}