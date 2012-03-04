using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Server
{
    public class ManagementServer : WebSocketServer<ManagementSession>
    {
        private IServerContainer m_ServerContainer;

        private string m_ManagePassword;

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<WebSocketRequestInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            var password = config.Options.GetValue("managePassword");

            if (string.IsNullOrEmpty(password))
            {
                Logger.Error("managePassword is required in configuration!");
                return false;
            }

            m_ManagePassword = password;

            return true;
        }

        protected override void OnStartup()
        {
            m_ServerContainer = GetService<IServerContainer>();
            m_ServerContainer.Loaded += new EventHandler(m_ServerContainer_Loaded);
            m_ServerContainer.PerformanceDataCollected += new EventHandler<PermformanceDataEventArgs>(m_ServerContainer_PerformanceDataCollected);

            base.OnStartup();
        }

        protected override void OnStopped()
        {
            if (m_ServerContainer != null)
            {
                m_ServerContainer.Loaded -= new EventHandler(m_ServerContainer_Loaded);
                m_ServerContainer.PerformanceDataCollected -= new EventHandler<PermformanceDataEventArgs>(m_ServerContainer_PerformanceDataCollected);
                m_ServerContainer = null;
            }

            base.OnStopped();
        }

        private ServerState m_ServerState;

        internal ServerInfo CurrentServerInfo { get; private set; }

        void m_ServerContainer_PerformanceDataCollected(object sender, PermformanceDataEventArgs e)
        {
            m_ServerState.GlobalPerformance = e.GlobalData;

            var performanceDict = new Dictionary<string, PerformanceData>(e.InstancesData.Length, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < e.InstancesData.Length; i++)
            {
                var p = e.InstancesData[i];
                performanceDict.Add(p.ServerName, p.Data);
            }

            for (var i = 0; i < m_ServerState.InstanceStates.Length; i++)
            {
                var s = m_ServerState.InstanceStates[i];

                PerformanceData p;

                if (performanceDict.TryGetValue(s.Instance.Name, out p))
                    s.Performance = p;
            }

            CurrentServerInfo = m_ServerState.ToServerInfo();
        }

        void m_ServerContainer_Loaded(object sender, EventArgs e)
        {
            m_ServerState = new ServerState
            {
                InstanceStates = m_ServerContainer.GetAllServers().Where(s => s != this).Select(s => new InstanceState
                {
                    Instance = s
                }).ToArray()
            };
        }

        internal IAppServer GetServerByName(string name)
        {
            return m_ServerContainer.GetServerByName(name);
        }
    }
}
