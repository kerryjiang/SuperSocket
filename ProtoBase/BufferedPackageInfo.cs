using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Buffered package info
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class BufferedPackageInfo<TKey> : IPackageInfo<TKey>, IBufferedPackageInfo
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedPackageInfo{TKey}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        public BufferedPackageInfo(TKey key, IList<ArraySegment<byte>> data)
        {
            Key = key;
            Data = data;
        }

        /// <summary>
        /// Gets the buffered data.
        /// </summary>
        /// <value>
        /// The buffered data.
        /// </value>
        public IList<ArraySegment<byte>> Data { get; private set; }
    }

    /// <summary>
    /// Buffered package info
    /// </summary>
    public class BufferedPackageInfo : BufferedPackageInfo<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedPackageInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        public BufferedPackageInfo(string key, IList<ArraySegment<byte>> data)
            : base(key, data)
        {

        }
    }
}
