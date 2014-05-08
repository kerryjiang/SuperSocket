using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    public interface ITransactionItem
    {
        void Rollback();
    }
}
