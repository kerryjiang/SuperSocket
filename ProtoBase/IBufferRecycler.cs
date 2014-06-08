using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The buffer recycler interface
    /// </summary>
    public interface IBufferRecycler
    {
        /// <summary>
        /// Returns the specified buffers.
        /// </summary>
        /// <param name="buffers">The buffers.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void Return(IList<KeyValuePair<ArraySegment<byte>, IBufferState>> buffers, int offset, int length);
    }

    /// <summary>
    /// The buffer recycler which do nothings
    /// </summary>
    class NullBufferRecycler : IBufferRecycler
    {
        public void Return(IList<KeyValuePair<ArraySegment<byte>, IBufferState>> buffers, int offset, int length)
        {
            //Do nothing
        }
    }
}
