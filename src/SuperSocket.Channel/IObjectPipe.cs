using System;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public interface IObjectPipe<T>
    {
        /// <summary>
        /// Write an object into the pipe
        /// </summary>
        /// <param name="target">the object tp be added into the pipe</param>
        /// <returns>pipe's length, how many objects left in the pipe</returns>
        int Write(T target);

        ValueTask<T> ReadAsync();
    }
}
