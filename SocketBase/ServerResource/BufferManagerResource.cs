using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ServerResource
{
    class BufferManagerResource : ServerResourceItem<IBufferManager>
    {
        public BufferManagerResource()
            : base("BufferPool")
        {

        }

        //Attach default receive buffer pool definition
        private List<IBufferPoolConfig> AttachReceiveBufferPool(IEnumerable<IBufferPoolConfig> bufferPools, IServerConfig config)
        {
            //The buffer expading trend: 1, 2, 4, 8, which is 15 totaly
            var initialCount = Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber);

            var bufferDefinitions = new List<IBufferPoolConfig>();

            IBufferPoolConfig preDefinedReceiveBufferPool = null;

            if (bufferPools != null && bufferPools.Any())
            {
                preDefinedReceiveBufferPool = bufferPools.FirstOrDefault(p => p.BufferSize == config.ReceiveBufferSize);
                bufferDefinitions.AddRange(bufferPools.Where(p => p != preDefinedReceiveBufferPool));
            }

            var finalInitialCount = initialCount;
            if (preDefinedReceiveBufferPool != null)
                finalInitialCount += preDefinedReceiveBufferPool.InitialCount;

            bufferDefinitions.Add(new BufferPoolConfig(config.ReceiveBufferSize, finalInitialCount));
            return bufferDefinitions;
        }

        protected override IBufferManager CreateResource(IServerConfig config)
        {
            //Initialize buffer manager
            var bufferDefinitions = AttachReceiveBufferPool(config.BufferPools, config);
            return new Pool.BufferManager(bufferDefinitions.ToArray());
        }
    }
}
