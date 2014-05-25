using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IBufferRecycler
    {
        void Return(IList<KeyValuePair<ArraySegment<byte>, IBufferState>> buffers, int offset, int length);
    }

    class NullBufferRecycler : IBufferRecycler
    {
        public void Return(IList<KeyValuePair<ArraySegment<byte>, IBufferState>> buffers, int offset, int length)
        {
            //Do nothing
        }
    }
}
