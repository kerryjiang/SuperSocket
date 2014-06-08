using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Pakcage info class template with key and body parts
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TBody">The type of the body.</typeparam>
    public class PackageInfo<TKey, TBody> : IPackageInfo<TKey>
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; private set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public TBody Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInfo{TKey, TBody}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        public PackageInfo(TKey key, TBody body)
        {
            Key = key;
            Body = body;
        }
    }
}
