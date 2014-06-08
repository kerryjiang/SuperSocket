using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The basic package info interface
    /// </summary>
    public interface IPackageInfo
    {

    }

    /// <summary>
    /// The basic package info interface with key
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IPackageInfo<out TKey> : IPackageInfo
    {
        /// <summary>
        /// Gets the key of the package info
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        TKey Key { get; }
    }
}
