using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Defines the basic functionality of a pipeline filter.
    /// </summary>
    public interface IPipelineFilter
    {
        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets or sets the context associated with the pipeline filter.
        /// </summary>
        object Context { get; set; }        
    }

    /// <summary>
    /// Defines the functionality of a pipeline filter for processing packages of a specific type.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPipelineFilter<TPackageInfo> : IPipelineFilter
    {
        /// <summary>
        /// Gets or sets the package decoder used by the pipeline filter.
        /// </summary>
        IPackageDecoder<TPackageInfo> Decoder { get; set; }

        /// <summary>
        /// Filters the data and extracts a package from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The extracted package, or <c>null</c> if more data is needed.</returns>
        TPackageInfo Filter(ref SequenceReader<byte> reader);

        /// <summary>
        /// Gets the next pipeline filter in the chain.
        /// </summary>
        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}