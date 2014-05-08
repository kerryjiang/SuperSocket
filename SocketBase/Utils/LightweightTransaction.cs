using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    public class LightweightTransaction : ITransactionGroup, IDisposable
    {
        private List<ITransactionItem> m_Items = new List<ITransactionItem>();

        private bool m_Commited = false;

        public void RegisterItem(ITransactionItem item)
        {
            m_Items.Add(item);
        }

        public void Commit()
        {
            m_Commited = true;
        }

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

        public void Dispose()
        {
            Dispose(true);
        }


        public ITransactionItem[] Items
        {
            get { return m_Items.ToArray(); }
        }
    }
}
