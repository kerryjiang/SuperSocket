using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Buffer pool configuration model class
    /// </summary>
    public class BufferPoolConfig : IBufferPoolConfig
    {
        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        /// <value>
        /// The size of the buffer.
        /// </value>
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets the initial count.
        /// </summary>
        /// <value>
        /// The initial count.
        /// </value>
        public int InitialCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPoolConfig"/> class.
        /// </summary>
        public BufferPoolConfig()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPoolConfig"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="initialCount">The initial count.</param>
        public BufferPoolConfig(int bufferSize, int initialCount)
        {
            BufferSize = bufferSize;
            InitialCount = initialCount;
        }
    }
}
