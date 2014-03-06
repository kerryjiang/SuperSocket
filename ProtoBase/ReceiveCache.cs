using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public sealed class ReceiveCache
    {
        public ArraySegment<byte> Current { get; set; }

        public List<ArraySegment<byte>> All { get; private set; }

        public ReceiveCache()
        {
            All = new List<ArraySegment<byte>>();
        }

        public int Total
        {
            get
            {
                var total = 0;
                var all = All;

                for (var i = 0; i < all.Count; i++)
                {
                    total += all[i].Count;
                }

                return total;
            }
        }

        public void SetLastLength(int length)
        {
            var all = All;
            var lastPos = all.Count - 1;
            var last = all[lastPos];
            all[lastPos] = new ArraySegment<byte>(last.Array, last.Offset, length);
        }
    }
}
