using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackageDecoder<TPackageInfo>
        where TPackageInfo : class
    {
        TPackageInfo Decode(ReadOnlySequence<byte> buffer);
    }
}