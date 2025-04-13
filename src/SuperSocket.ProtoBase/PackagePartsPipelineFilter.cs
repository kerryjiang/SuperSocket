using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// An abstract pipeline filter that processes packages in parts using a sequence of part readers.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public abstract class PackagePartsPipelineFilter<TPackageInfo> : IPipelineFilter<TPackageInfo>
    {
        private IPackagePartReader<TPackageInfo> _currentPartReader;

        /// <summary>
        /// Gets the current package being processed.
        /// </summary>
        protected TPackageInfo CurrentPackage { get; private set; }

        /// <summary>
        /// Creates a new package instance.
        /// </summary>
        /// <returns>The created package.</returns>
        protected abstract TPackageInfo CreatePackage();

        /// <summary>
        /// Gets the first part reader to start processing the package.
        /// </summary>
        /// <returns>The first part reader.</returns>
        protected abstract IPackagePartReader<TPackageInfo> GetFirstPartReader();

        /// <summary>
        /// Filters the data and processes the package in parts.
        /// </summary>
        /// <param name="reader">The sequence reader containing the data.</param>
        /// <returns>The processed package, or <c>null</c> if more data is needed.</returns>
        public virtual TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var package = CurrentPackage;
            var currentPartReader = _currentPartReader;

            if (package == null)
            {
                package = CurrentPackage = CreatePackage();
                currentPartReader = _currentPartReader = GetFirstPartReader();
            }

            while (true)
            {
                if (currentPartReader.Process(package, Context, ref reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData))
                {
                    Reset();
                    return package;
                }

                if (nextPartReader != null)
                {
                    _currentPartReader = nextPartReader;
                    OnPartReaderSwitched(currentPartReader, nextPartReader);
                    currentPartReader = nextPartReader;
                }

                if (needMoreData || reader.Remaining <= 0)
                    return default;
            }
        }

        /// <summary>
        /// Called when the part reader is switched to a new one.
        /// </summary>
        /// <param name="currentPartReader">The current part reader.</param>
        /// <param name="nextPartReader">The next part reader.</param>
        protected virtual void OnPartReaderSwitched(IPackagePartReader<TPackageInfo> currentPartReader, IPackagePartReader<TPackageInfo> nextPartReader)
        {
        }

        /// <summary>
        /// Resets the state of the pipeline filter.
        /// </summary>
        public virtual void Reset()
        {
            CurrentPackage = default;
            _currentPartReader = null;
        }

        /// <summary>
        /// Gets or sets the context associated with the pipeline filter.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets or sets the package decoder used by the pipeline filter.
        /// </summary>
        public IPackageDecoder<TPackageInfo> Decoder { get; set; }

        /// <summary>
        /// Gets or sets the next pipeline filter in the chain.
        /// </summary>
        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }
    }
}