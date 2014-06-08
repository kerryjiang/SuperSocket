using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    /// <summary>
    /// The basic pool interface
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        int TotalCount { get; }

        /// <summary>
        /// Gets the available count, the items count which are available to be used.
        /// </summary>
        /// <value>
        /// The available count.
        /// </value>
        int AvailableCount { get; }

        /// <summary>
        /// Shrinks this pool.
        /// </summary>
        /// <returns></returns>
        bool Shrink();
    }

    /// <summary>
    /// The basic pool interface for the object in type of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T> : IPool
    {
        /// <summary>
        /// Gets one item from the pool.
        /// </summary>
        /// <returns></returns>
        T Get();

        /// <summary>
        /// Returns the specified item to the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        void Return(T item);
    }
}
