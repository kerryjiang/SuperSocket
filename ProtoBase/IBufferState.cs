using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The buffer state interface
    /// </summary>
    public interface IBufferState
    {
        /// <summary>
        /// Decreases the reference count of this buffer state
        /// </summary>
        /// <returns></returns>
        int DecreaseReference();

        /// <summary>
        /// Increases the reference count of this buffer state
        /// </summary>
        void IncreaseReference();
    }

    /// <summary>
    /// The buffer state base class
    /// </summary>
    public abstract class BufferBaseState : IBufferState
    {
        private int m_Reference;

        /// <summary>
        /// Decreases the reference count of this buffer state
        /// </summary>
        /// <returns></returns>
        public int DecreaseReference()
        {
            return Interlocked.Decrement(ref m_Reference) - 1;
        }

        /// <summary>
        /// Increases the reference count of this buffer state
        /// </summary>
        public void IncreaseReference()
        {
            Interlocked.Increment(ref m_Reference);
        }
    }
}
