using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    /// <summary>
    /// The interface for transaction item
    /// </summary>
    public interface ITransactionItem
    {
        /// <summary>
        /// Rollbacks the transaction item.
        /// </summary>
        void Rollback();
    }
}
