using System;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public interface IObjectPipe<T>
    {
        void Write(T target);

        ValueTask<T> ReadAsync();
    }
}
