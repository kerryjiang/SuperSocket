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
        int TotalCount { get; }

        int AvailableCount { get; }

        void Shrink();
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
