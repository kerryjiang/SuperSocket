using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Runtime memory assignment buffer manager (no buffer)
    /// </summary>
    public class NullBufferManager : IBufferManager
    {
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <param name="size">The size of the resired buffer.</param>
        /// <returns></returns>
        public byte[] GetBuffer(int size)
        {
            return new byte[size];
        }

        /// <summary>
        /// Returns the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void ReturnBuffer(byte[] buffer)
        {
            // do nothing
        }

        /// <summary>
        /// Shrinks this instance.
        /// </summary>
        public void Shrink()
        {
            // do nothing
        }
    }
}
