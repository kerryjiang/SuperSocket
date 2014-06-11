using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The raw package info interface
    /// </summary>
    public interface IRawPackageInfo
    {
        /// <summary>
        /// Gets the raw data.
        /// </summary>
        /// <value>
        /// The raw data.
        /// </value>
        BufferList RawData { get; }
    }

    /// <summary>
    /// The default raw package info class
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class RawPackageInfo<TKey> : IPackageInfo<TKey>, IRawPackageInfo
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; private set; }

        /// <summary>
        /// Gets the raw data.
        /// </summary>
        /// <value>
        /// The raw data.
        /// </value>
        public BufferList RawData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawPackageInfo{TKey}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rawData">The raw data.</param>
        public RawPackageInfo(TKey key, BufferList rawData)
        {
            Key = key; ;
            RawData = rawData;
        }
    }
}
