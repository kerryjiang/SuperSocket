using System;

namespace SuperSocket.ProtoBase
{
    public interface IKeyedPackageInfo<TKey>
    {
        TKey Key { get; }
    }
}
