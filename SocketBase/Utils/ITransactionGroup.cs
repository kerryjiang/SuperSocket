using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    public interface ITransactionGroup
    {
        void RegisterItem(ITransactionItem item);

        void Commit();

        ITransactionItem[] Items { get; }
    }
}
