using System;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The buffer manager interface
    /// </summary>
    public interface IBufferManager
    {
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <param name="size">The size of the resired buffer.</param>
        /// <returns></returns>
        byte[] GetBuffer(int size);

        /// <summary>
        /// Returns the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        void ReturnBuffer(byte[] buffer);
    }
}
