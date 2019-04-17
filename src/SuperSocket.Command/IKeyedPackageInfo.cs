using System;

namespace SuperSocket.Command
{
    public interface IKeyedPackageInfo<TKey>
    {
        TKey Key { get; }
    }
}
