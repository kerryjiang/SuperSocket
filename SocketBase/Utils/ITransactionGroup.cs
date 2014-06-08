using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    /// <summary>
    /// The interface for group contains many transactional items
    /// </summary>
    public interface ITransactionGroup
    {
        /// <summary>
        /// Registers the item.
        /// </summary>
        /// <param name="item">The item.</param>
        void RegisterItem(ITransactionItem item);

        /// <summary>
        /// Commits this transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Gets all the transaction items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        ITransactionItem[] Items { get; }
    }
}
