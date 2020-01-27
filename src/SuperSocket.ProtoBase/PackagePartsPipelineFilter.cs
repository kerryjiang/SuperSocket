using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class PackagePartsPipelineFilter<TPackageInfo> : IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        private IPackagePartReader<TPackageInfo> _currentPartReader;

        private TPackageInfo _currentPackage;

        protected abstract TPackageInfo CreatePackage();

        protected abstract IPackagePartReader<TPackageInfo> GetFirstPartReader();

        public virtual TPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var package = _currentPackage;

            if (package == null)
            {
                package = _currentPackage = CreatePackage();
                _currentPartReader = GetFirstPartReader();
            }

            while (true)
            {
                if (_currentPartReader.Process(package, ref reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData))
                {
                    Reset();
                    return package;
                }

                if (nextPartReader != null)
                    _currentPartReader = nextPartReader;

                if (needMoreData || reader.Remaining <= 0)
                    return null;
            }
        }

        public virtual void Reset()
        {
            _currentPackage = null;
            _currentPartReader = null;
        }

        public object Context { get; set; }

        public IPackageDecoder<TPackageInfo> Decoder { get; set; }

        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }
    }
}