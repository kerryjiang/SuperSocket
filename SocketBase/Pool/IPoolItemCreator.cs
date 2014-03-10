using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    public interface IPoolItemCreator<T>
    {
        IEnumerable<T> Create(int count);
    }
}
