using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    public interface IPoolableItem
    {
        int Generation { get; }
    }

    public interface IPoolableItem<T> : IPoolableItem
        where T : IPoolableItem
    {
        IPool<T> Pool { get; }
    }
}
