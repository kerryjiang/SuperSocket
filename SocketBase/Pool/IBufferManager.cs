using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    /// <summary>
    /// The buffer manager interface
    /// </summary>
    public interface IBufferManager
    {
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        byte[] GetBuffer(int size);

        /// <summary>
        /// Returns the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        void ReturnBuffer(byte[] buffer);

        /// <summary>
        /// Shrinks this instance.
        /// </summary>
        void Shrink();
    }
}
