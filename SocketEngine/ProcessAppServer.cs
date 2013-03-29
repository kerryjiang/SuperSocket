using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    class ProcessAppServer : MarshalByRefObject, IWorkItem
    {
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            throw new NotImplementedException();
        }

        public bool Start()
        {
            throw new NotImplementedException();
        }

        public ServerState State
        {
            get { throw new NotImplementedException(); }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public int SessionCount
        {
            get { throw new NotImplementedException(); }
        }

        public ServerSummary Summary
        {
            get { throw new NotImplementedException(); }
        }

        public ServerSummary CollectServerSummary(NodeSummary nodeSummary)
        {
            throw new NotImplementedException();
        }
    }
}
