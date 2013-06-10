using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketEngine
{
    class RemoteBootstrapProxy : MarshalByRefObject, IBootstrap
    {
        private IBootstrap m_Bootstrap;

        public RemoteBootstrapProxy()
        {
            m_Bootstrap = (IBootstrap)AppDomain.CurrentDomain.GetData("Bootstrap");
        }

        public IEnumerable<IWorkItem> AppServers
        {
            get { return m_Bootstrap.AppServers; }
        }

        public IRootConfig Config
        {
            get { return m_Bootstrap.Config; }
        }

        public bool Initialize()
        {
            throw new NotSupportedException();
        }

        public bool Initialize(IDictionary<string, System.Net.IPEndPoint> listenEndPointReplacement)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(ILogFactory logFactory)
        {
            throw new NotSupportedException();
        }

        public bool Initialize(Func<IServerConfig, IServerConfig> serverConfigResolver, ILogFactory logFactory)
        {
            throw new NotSupportedException();
        }

        public StartResult Start()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            throw new NotSupportedException();
        }

        public string StartupConfigFile
        {
            get { return m_Bootstrap.StartupConfigFile; }
        }

        public string BaseDirectory
        {
            get { return m_Bootstrap.BaseDirectory; }
        }

        public override object InitializeLifetimeService()
        {
            //Never expire
            return null;
        }
    }
}
