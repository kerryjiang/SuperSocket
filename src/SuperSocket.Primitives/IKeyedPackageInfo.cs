using System;

namespace SuperSocket
{
    public interface IKeyedPackageInfo<TKey>
    {
        TKey Key { get; }
    }
}
