using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.SocksServer
{
    public class SocksServer : AppServer<SocksSession, BinaryCommandInfo>
    {
        public SocksServer()
            : base(new SocksProtocol())
        {
            
        }

        private BufferManager m_BufferManager;
        private ConcurrentStack<SocketAsyncEventArgs> m_SocketAsyncPool;

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<BinaryCommandInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            int bufferSize = Math.Max(config.ReceiveBufferSize, config.SendBufferSize);

            if (bufferSize <= 0)
                bufferSize = 1024 * 8;

            m_BufferManager = new BufferManager(bufferSize * config.MaxConnectionNumber, bufferSize);

            try
            {
                m_BufferManager.InitBuffer();
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                return false;
            }

            var socketArgsList = new List<SocketAsyncEventArgs>(config.MaxConnectionNumber);

            for (int i = 0; i < config.MaxConnectionNumber; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                m_BufferManager.SetBuffer(socketEventArg);
                socketArgsList.Add(socketEventArg);
            }

            m_SocketAsyncPool = new ConcurrentStack<SocketAsyncEventArgs>(socketArgsList);

            return true;
        }

        internal SocketAsyncEventArgs GetSocketAsyncEventArgs()
        {
            SocketAsyncEventArgs socketAsyncEventArgs;
            if (m_SocketAsyncPool.TryPop(out socketAsyncEventArgs))
                return socketAsyncEventArgs;
            else
                return null;
        }

        internal void ReleaseSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            m_SocketAsyncPool.Push(e);
        }
    }
}
