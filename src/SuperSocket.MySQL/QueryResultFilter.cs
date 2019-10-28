using System;
using System.Buffers;
using SuperSocket.ProtoBase;
using SuperSocket.MySQL.PackagePartReader;

namespace SuperSocket.MySQL
{
    public class QueryResultFilter : IPipelineFilter<QueryResult>
    {
        public IPackageDecoder<QueryResult> Decoder { get; set; } 

        public IPipelineFilter<QueryResult> NextFilter => null;

        private IPackagePartReader _currentPartReader;

        private QueryResult _currentPackage;

        public QueryResult Filter(ref SequenceReader<byte> reader)
        {
            var package = _currentPackage;

            if (package == null)
            {
                package = _currentPackage = new QueryResult();
                _currentPartReader = PackagePartReader.PackagePartReader.NewReader;
            }

            while (true)
            {
                if (_currentPartReader.Process(package, ref reader, out IPackagePartReader nextPartReader, out bool needMoreData))
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

        public void Reset()
        {
            _currentPackage = null;
            _currentPartReader = null;
        }
    }
}
