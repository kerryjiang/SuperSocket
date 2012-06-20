using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Server configruation model
    /// </summary>
    public class ServerConfig : IServerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConfig"/> class.
        /// </summary>
        public ServerConfig()
        {
            Security = "None";
            MaxConnectionNumber = 100;
            Mode = SocketMode.Tcp;
            MaxRequestLength = 1024;
            KeepAliveTime = 10 * 60;// 10 minutes
            KeepAliveInterval = 60;// 60 seconds
            ListenBacklog = 100;
        }

        #region IServerConfig Members

        /// <summary>
        /// Gets/sets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets/sets the protocol.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets/sets the ip.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets/sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets/sets the options.
        /// </summary>
        public NameValueCollection Options { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether this <see cref="IServerConfig"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the mode.
        /// </summary>
        public SocketMode Mode { get; set; }

        /// <summary>
        /// Gets/sets the send time out.
        /// </summary>
        public int SendTimeOut { get; set; }

        /// <summary>
        /// Gets the max connection number.
        /// </summary>
        public int MaxConnectionNumber { get; set; }

        /// <summary>
        /// Gets the size of the receive buffer.
        /// </summary>
        /// <value>
        /// The size of the receive buffer.
        /// </value>
        public int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets the size of the send buffer.
        /// </summary>
        /// <value>
        /// The size of the send buffer.
        /// </value>
        public int SendBufferSize { get; set; }


        /// <summary>
        /// Gets a value indicating whether sending is in synchronous mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sync send]; otherwise, <c>false</c>.
        /// </value>
        public bool SyncSend { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether log command in log file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if log command; otherwise, <c>false</c>.
        /// </value>
        public bool LogCommand { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether clear idle session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if clear idle session; otherwise, <c>false</c>.
        /// </value>
        public bool ClearIdleSession { get; set; }

        /// <summary>
        /// Gets/sets the clear idle session interval, in seconds.
        /// </summary>
        /// <value>
        /// The clear idle session interval.
        /// </value>
        public int ClearIdleSessionInterval { get; set; }

        /// <summary>
        /// Gets/sets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>
        /// The idle session time out.
        /// </value>
        public int IdleSessionTimeOut { get; set; }

        /// <summary>
        /// Gets/sets X509Certificate configuration.
        /// </summary>
        /// <value>
        /// X509Certificate configuration.
        /// </value>
        public ICertificateConfig Certificate { get; set; }

        /// <summary>
        /// Gets/sets the security protocol, X509 certificate.
        /// </summary>
        public string Security { get; set; }

        /// <summary>
        /// Gets/sets the length of the max request.
        /// </summary>
        /// <value>
        /// The length of the max request.
        /// </value>
        public int MaxRequestLength { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether [disable session snapshot].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable session snapshot]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableSessionSnapshot { get; set; }

        /// <summary>
        /// Gets/sets the interval to taking snapshot for all live sessions.
        /// </summary>
        public int SessionSnapshotInterval { get; set; }

        /// <summary>
        /// Gets/sets the connection filters used by this server instance.
        /// </summary>
        /// <value>
        /// The connection filters's name list, seperated by comma
        /// </value>
        public string ConnectionFilters { get; set; }

        /// <summary>
        /// Gets/sets the start keep alive time, in seconds
        /// </summary>
        public int KeepAliveTime { get; set; }

        /// <summary>
        /// Gets/sets the keep alive interval, in seconds.
        /// </summary>
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether [enable dynamic command](support commands written in IronPython).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [dynamic command is enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableDynamicCommand { get; set; }

        /// <summary>
        /// Gets the backlog size of socket listening.
        /// </summary>
        public int ListenBacklog { get; set; }

        /// <summary>
        /// Gets/sets the startup order of the server instance.
        /// </summary>
        public int StartupOrder { get; set; }

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public virtual TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return default(TConfig);
        }

        /// <summary>
        /// Gets and sets the listeners' configuration.
        /// </summary>
        public IEnumerable<IListenerConfig> Listeners { get; set; }

        #endregion
    }
}
