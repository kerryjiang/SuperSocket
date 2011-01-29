using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Authentication;

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
        }

        #region IServerConfig Members

        public string ServiceName { get; set; }

        public string Protocol { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }

        public NameValueConfigurationCollection Parameters { get; set; }

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

        public int SessionSnapshotInterval { get; set; }

        #endregion
    }
}
