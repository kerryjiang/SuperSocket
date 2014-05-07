using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Utils
{
    interface ITransactionItem
    {
        void Rollback();
    }
}
