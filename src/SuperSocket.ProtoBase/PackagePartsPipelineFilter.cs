using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class PackagePartsPipelineFilter<TPackageInfo> : IPipelineFilter<TPackageInfo>
    {
        private IPackagePartReader<TPackageInfo> _currentPartReader;

        protected TPackageInfo CurrentPackage { get; private set; }

        protected abstract TPackageInfo CreatePackage();

        protected abstract IPackagePartReader<TPackageInfo> GetFirstPartReader();

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

        protected virtual void OnPartReaderSwitched(IPackagePartReader<TPackageInfo> currentPartReader, IPackagePartReader<TPackageInfo> nextPartReader)
        {

        }

        public virtual void Reset()
        {
            CurrentPackage = default;
            _currentPartReader = null;
        }

        public object Context { get; set; }

        public IPackageDecoder<TPackageInfo> Decoder { get; set; }

        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }
    }
}