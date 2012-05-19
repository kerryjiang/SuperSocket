using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.Management.Server.Config;
using SuperSocket.Management.Shared;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperWebSocket;
using SuperWebSocket.Protocol;
using SuperWebSocket.SubProtocol;

namespace SuperSocket.Management.Server
{
    /// <summary>
    /// Server manager app server
    /// </summary>
    public class ManagementServer : WebSocketServer<ManagementSession>
    {
        private Dictionary<string, UserConfig> m_UsersDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementServer"/> class.
        /// </summary>
        public ManagementServer()
            : base(new BasicSubProtocol<ManagementSession>("ServerManager"))
        {

        }

        /// <summary>
        /// Setups with the specified parameters.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <param name="socketServerFactory">The socket server factory.</param>
        /// <param name="protocol">The protocol.</param>
        /// <returns></returns>
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<IWebSocketFragment> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            var users = config.GetChildConfig<UserConfigCollection>("users");

            if (users == null || users.Count <= 0)
            {
                Logger.Error("No user defined");
                return false;
            }

            m_UsersDict = new Dictionary<string, UserConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var u in users)
            {
                m_UsersDict.Add(u.Name, u);
            }

            return true;
        }

        /// <summary>
        /// Called when [startup].
        /// </summary>
        protected override void OnStartup()
        {
            m_ServerState = new ServerState
            {
                InstanceStates = Bootstrap.AppServers.Where(s => s != this).Select(s => new InstanceState
                {
                    Instance = s
                }).ToArray()
            };

            Bootstrap.PerformanceDataCollected += new EventHandler<PermformanceDataEventArgs>(BootstrapPerformanceDataCollected);
            base.OnStartup();
        }

        /// <summary>
        /// Called when [stopped].
        /// </summary>
        protected override void OnStopped()
        {
            Bootstrap.PerformanceDataCollected -= new EventHandler<PermformanceDataEventArgs>(BootstrapPerformanceDataCollected);
            base.OnStopped();
        }

        private ServerState m_ServerState;

        internal ServerInfo CurrentServerInfo { get; private set; }

        void BootstrapPerformanceDataCollected(object sender, PermformanceDataEventArgs e)
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

            var content = CommandName.UPDATE + " " + JsonConvert.SerializeObject(CurrentServerInfo);

            foreach (var s in GetSessions(s => s.LoggedIn))
            {
                s.SendResponse(content);
            }
        }

        internal ServerInfo GetUpdatedCurrentServerInfo()
        {
            CurrentServerInfo = m_ServerState.ToServerInfo();
            return CurrentServerInfo;
        }

        internal IAppServer GetServerByName(string name)
        {
            return Bootstrap.AppServers.FirstOrDefault(i => name.Equals(i.Name, StringComparison.OrdinalIgnoreCase));
        }

        internal UserConfig GetUserByName(string name)
        {
            UserConfig user;
            m_UsersDict.TryGetValue(name, out user);
            return user;
        }
    }
}
