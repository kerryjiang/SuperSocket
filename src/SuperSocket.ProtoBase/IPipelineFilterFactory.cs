namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Defines a factory for creating pipeline filters for specific package types.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPipelineFilterFactory<TPackageInfo>
    {
        /// <summary>
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter.</returns>
        IPipelineFilter<TPackageInfo> Create(object client);
    }

    /// <summary>
    /// Defines a factory for creating general pipeline filters.
    /// </summary>
    public interface IPipelineFilterFactory
    {
        /// <summary>
        /// Creates a pipeline filter for the specified client.
        /// </summary>
        /// <param name="client">The client for which the pipeline filter is created.</param>
        /// <returns>The created pipeline filter.</returns>
        IPipelineFilter Create(object client);
    }
}