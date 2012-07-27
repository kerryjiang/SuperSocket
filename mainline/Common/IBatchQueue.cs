using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    /// <summary>
    /// A queue interface which can operate in batch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchQueue<T>
    {
        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        bool Enqueue(T item);

        /// <summary>
        /// Enqueues the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        bool Enqueue(IList<T> items);

        /// <summary>
        /// Tries to dequeue all items in the queue into the output list.
        /// </summary>
        /// <param name="outputItems">The output items.</param>
        /// <returns></returns>
        bool TryDequeue(IList<T> outputItems);

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        int Count { get; }
    }
}
