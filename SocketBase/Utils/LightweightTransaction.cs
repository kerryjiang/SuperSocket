using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    /// <summary>
    /// The light weight transaction
    /// </summary>
    public class LightweightTransaction : ITransactionGroup, IDisposable
    {
        private List<ITransactionItem> m_Items = new List<ITransactionItem>();

        private bool m_Commited = false;

        /// <summary>
        /// Registers the item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void RegisterItem(ITransactionItem item)
        {
            m_Items.Add(item);
        }

        /// <summary>
        /// Commits this transaction.
        /// </summary>
        public void Commit()
        {
            m_Commited = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LightweightTransaction"/> class.
        /// </summary>
        ~LightweightTransaction()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!m_Commited)
                {
                    m_Items.ForEach(i => i.Rollback());
                }

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }


        /// <summary>
        /// Gets all the transaction items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public ITransactionItem[] Items
        {
            get { return m_Items.ToArray(); }
        }
    }
}
