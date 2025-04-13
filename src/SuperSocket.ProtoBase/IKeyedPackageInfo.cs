using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents a package with an associated key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IKeyedPackageInfo<TKey>
    {
        /// <summary>
        /// Gets the key associated with the package.
        /// </summary>
        TKey Key { get; }
    }
}
