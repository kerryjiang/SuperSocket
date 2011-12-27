using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Config
{
    public class ServerConfig : IServerConfig
    {
        public ServerConfig()
        {
            Security = "None";
            MaxConnectionNumber = 100;
            Mode = SocketMode.Async;
            MaxCommandLength = 1024;
            KeepAliveTime = 10 * 60;// 10 minutes
            KeepAliveInterval = 60;// 60 seconds
            ListenBacklog = 100;
        }

        #region IServerConfig Members

        public string ServiceName { get; set; }

        public string Protocol { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }

        public NameValueCollection Options { get; set; }

        public string Provider { get; set; }

        public bool Disabled { get; set; }

        public string Name { get; set; }

        public SocketMode Mode { get; set; }

        public bool EnableManagementService { get; set; }

        public int ReadTimeOut { get; set; }

        public int SendTimeOut { get; set; }

        public int MaxConnectionNumber { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        /// <summary>
        /// Gets a value indicating whether log command in log file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if log command; otherwise, <c>false</c>.
        /// </value>
        public bool LogCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether clear idle session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if clear idle session; otherwise, <c>false</c>.
        /// </value>
        public bool ClearIdleSession { get; set; }

        /// <summary>
        /// Gets the clear idle session interval, in seconds.
        /// </summary>
        /// <value>
        /// The clear idle session interval.
        /// </value>
        public int ClearIdleSessionInterval { get; set; }

        /// <summary>
        /// Gets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>
        /// The idle session time out.
        /// </value>
        public int IdleSessionTimeOut { get; set; }

        public ICertificateConfig Certificate { get; set; }

        public string Security { get; set; }

        public int MaxCommandLength { get; set; }

        /// <summary>
        /// Gets a value indicating whether [disable session snapshot].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable session snapshot]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableSessionSnapshot { get; set; }

        public int SessionSnapshotInterval { get; set; }

        public string ConnectionFilters { get; set; }

        /// <summary>
        /// Gets the start keep alive time, in seconds
        /// </summary>
        public int KeepAliveTime { get; set; }

        /// <summary>
        /// Gets the keep alive interval, in seconds.
        /// </summary>
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// Gets the backlog size of socket listening.
        /// </summary>
        public int ListenBacklog { get; set; }


        public virtual TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return default(TConfig);
        }

        #endregion
    }
}
