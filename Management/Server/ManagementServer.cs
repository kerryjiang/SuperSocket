using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.Management.Server.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Protocol;
using SuperSocket.WebSocket.SubProtocol;

namespace SuperSocket.Management.Server
{
    /// <summary>
    /// Server manager app server
    /// </summary>
    public class ManagementServer : WebSocketServer<ManagementSession>
    {
        private Dictionary<string, UserConfig> m_UsersDict;

        private string[] m_ExcludedServers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementServer"/> class.
        /// </summary>
        public ManagementServer()
            : base(new BasicSubProtocol<ManagementSession>("ServerManager"))
        {

        }


        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            if (!base.Setup(rootConfig, config))
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

            m_ExcludedServers = config.Options.GetValue("excludedServers", string.Empty).Split(
                new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            return true;
        }

        private static JsonConverter m_IPEndPointConverter = new ListenersJsonConverter();

        /// <summary>
        /// Jsons the serialize.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override string JsonSerialize(object target)
        {
            return JsonConvert.SerializeObject(target, m_IPEndPointConverter);
        }
    }
}
