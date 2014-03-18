using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IBufferRecycler
    {
        void Return(IList<KeyValuePair<ArraySegment<byte>, object>> buffers, int offset, int length);
    }
}
