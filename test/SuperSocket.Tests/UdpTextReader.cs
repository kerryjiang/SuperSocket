using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Tests
{
    public class UdpTextReader : TextReader
    {
        public UdpPipeConnection<TextPackageInfo> Connection { get; }

        private IAsyncEnumerator<TextPackageInfo> _packageEnumerator;

        public UdpTextReader(UdpPipeConnection<TextPackageInfo> connection)
        {
            Connection = connection;
            _packageEnumerator = connection.RunAsync().GetAsyncEnumerator();
        }

        public override string ReadLine()
        {
            return ReadLineAsync().GetAwaiter().GetResult();
        }

        public async override Task<string> ReadLineAsync()
        {
            var ret = await _packageEnumerator.MoveNextAsync().ConfigureAwait(false);

            if (!ret)
                return null;

            return _packageEnumerator.Current?.Text;
        }
    }
}