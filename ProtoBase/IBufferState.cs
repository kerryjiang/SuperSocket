using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    public interface IBufferState
    {
        int DecreaseReference();

        void IncreaseReference();
    }

    public abstract class BufferBaseState : IBufferState
    {
        private int m_Reference;
        
        public int DecreaseReference()
        {
            return Interlocked.Decrement(ref m_Reference) - 1;
        }

        public void IncreaseReference()
        {
            Interlocked.Increment(ref m_Reference);
        }
    }
}
