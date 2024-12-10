using System;
using System.Threading.Tasks;

namespace SuperSocket.Connection
{
    public interface IObjectPipe
    {
        void WirteEOF();
    }

    public interface IObjectPipe<T> : IObjectPipe
    {
        /// <summary>
        /// Write an object into the pipe
        /// </summary>
        /// <param name="target">the object to be added into the pipe</param>
        /// <returns>pipe's length, how many objects left in the pipe</returns>
        int Write(T target);

        ValueTask<T> ReadAsync();
    }

    interface ISupplyController
    {
        ValueTask SupplyRequired();

        void SupplyEnd();
    }
}