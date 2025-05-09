using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// An abstract base class for creating pipeline filters.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class PipelineFilterBase<TPackageInfo> : IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        /// <summary>
        /// Gets or sets the next pipeline filter in the chain.
        /// </summary>
        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }

        /// <summary>
        /// Gets or sets the package decoder used by the pipeline filter.
        /// </summary>
        public IPackageDecoder<TPackageInfo> Decoder { get; set; }

        /// <summary>
        /// Gets or sets the context associated with the pipeline filter.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Filters the data and extracts a package from the sequence reader.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The extracted package, or <c>null</c> if more data is needed.</returns>
        public abstract TPackageInfo Filter(ref SequenceReader<byte> reader);

        /// <summary>
        /// Decodes a package from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the package data.</param>
        /// <returns>The decoded package.</returns>
        protected virtual TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return Decoder.Decode(ref buffer, Context);
        }

        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        public virtual void Reset()
        {
            if (NextFilter != null)
                NextFilter = null;
        }
    }
}