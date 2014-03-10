using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    public interface IPool<T>
    {
        int TotalCount { get; }

        int AvailableCount { get; }

        T Get();

        void Return(T item);

        void Shrink();
    }
}
