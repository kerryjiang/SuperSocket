using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IChannel
    {
        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package);

        void Close();
    }

    public interface IChannel<TPackageInfo> : IChannel
    {
        IAsyncEnumerable<TPackageInfo> RunAsync();
    }
}
