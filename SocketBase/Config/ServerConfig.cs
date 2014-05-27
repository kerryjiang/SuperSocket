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
    /// <summary>
    /// Server configruation model
    /// </summary>
    [Serializable]
    public partial class ServerConfig : IServerConfig
    {
        /// <summary>
        /// Default ReceiveBufferSize
        /// </summary>
        public const int DefaultReceiveBufferSize = 4096;

        /// <summary>
        /// Default MaxConnectionNumber
        /// </summary>
        public const int DefaultMaxConnectionNumber = 100;


        /// <summary>
        /// Default sending queue size
        /// </summary>
        public const int DefaultSendingQueueSize = 5;

        /// <summary>
        /// Default MaxRequestLength
        /// </summary>
        public const int DefaultMaxRequestLength = 1024;


        /// <summary>
        /// Default send timeout value, in milliseconds
        /// </summary>
        public const int DefaultSendTimeout = 5000;


        /// <summary>
        /// Default clear idle session interval
        /// </summary>
        public const int DefaultClearIdleSessionInterval = 120;


        /// <summary>
        /// Default idle session timeout
        /// </summary>
        public const int DefaultIdleSessionTimeOut = 300;


        /// <summary>
        /// The default send buffer size
        /// </summary>
        public const int DefaultSendBufferSize = 2048;


        /// <summary>
        /// The default session snapshot interval
        /// </summary>
        public const int DefaultSessionSnapshotInterval = 5;

        /// <summary>
        /// The default keep alive time
        /// </summary>
        public const int DefaultKeepAliveTime = 600; // 60 * 10 = 10 minutes


        /// <summary>
        /// The default keep alive interval
        /// </summary>
        public const int DefaultKeepAliveInterval = 60; // 60 seconds


        /// <summary>
        /// The default listen backlog
        /// </summary>
        public const int DefaultListenBacklog = 100;


        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConfig"/> class.
        /// </summary>
        /// <param name="serverConfig">The server config.</param>
        public ServerConfig(IServerConfig serverConfig)
        {
            serverConfig.CopyPropertiesTo(this);
            
            this.Options = serverConfig.Options;
            this.OptionElements = serverConfig.OptionElements;

            if (serverConfig.Certificate != null)
                this.Certificate = serverConfig.Certificate.CopyPropertiesTo(new CertificateConfig());

            if (serverConfig.Listeners != null && serverConfig.Listeners.Any())
            {
                this.Listeners = serverConfig.Listeners.Select(l => l.CopyPropertiesTo(new ListenerConfig())).OfType<ListenerConfig>().ToArray();
            }

            if (serverConfig.CommandAssemblies != null && serverConfig.CommandAssemblies.Any())
            {
                this.CommandAssemblies = serverConfig.CommandAssemblies.Select(c => c.CopyPropertiesTo(new CommandAssemblyConfig())).OfType<CommandAssemblyConfig>().ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConfig"/> class.
        /// </summary>
        public ServerConfig()
        {
            Security = "None";
            MaxConnectionNumber = DefaultMaxConnectionNumber;
            Mode = SocketMode.Tcp;
            MaxRequestLength = DefaultMaxRequestLength;
            KeepAliveTime = DefaultKeepAliveTime;
            KeepAliveInterval = DefaultKeepAliveInterval;
            ListenBacklog = DefaultListenBacklog;
            ReceiveBufferSize = DefaultReceiveBufferSize;
            SendingQueueSize = DefaultSendingQueueSize;
            SendTimeOut = DefaultSendTimeout;
            ClearIdleSessionInterval = DefaultClearIdleSessionInterval;
            IdleSessionTimeOut = DefaultIdleSessionTimeOut;
            SendBufferSize = DefaultSendBufferSize;
            LogBasicSessionActivity = true;
            SessionSnapshotInterval = DefaultSessionSnapshotInterval;
        }

        #region IServerConfig Members

        /// <summary>
        /// Gets/sets the name of the server type of this appServer want to use.
        /// </summary>
        /// <value>
        /// The name of the server type.
        /// </value>
        public string ServerTypeName { get; set; }


        /// <summary>
        /// Gets/sets the type definition of the appserver.
        /// </summary>
        /// <value>
        /// The type of the server.
        /// </value>
        public string ServerType { get; set; }

        /// <summary>
        /// Gets/sets the Receive filter factory.
        /// </summary>
        public string ReceiveFilterFactory { get; set; }

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
        /// Gets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; set; }

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
        /// The connection filter's name list, seperated by comma
        /// </value>
        public string ConnectionFilter { get; set; }

        /// <summary>
        /// Gets the command loader, multiple values should be separated by comma.
        /// </summary>
        public string CommandLoader { get; set; }

        /// <summary>
        /// Gets/sets the start keep alive time, in seconds
        /// </summary>
        public int KeepAliveTime { get; set; }

        /// <summary>
        /// Gets/sets the keep alive interval, in seconds.
        /// </summary>
        public int KeepAliveInterval { get; set; }

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
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }

        /// <summary>
        /// Gets and sets the listeners' configuration.
        /// </summary>
        public IEnumerable<IListenerConfig> Listeners { get; set; }

        /// <summary>
        /// Gets/sets the log factory name.
        /// </summary>
        public string LogFactory { get; set; }

        /// <summary>
        /// Gets/sets the size of the sending queue.
        /// </summary>
        /// <value>
        /// The size of the sending queue.
        /// </value>
        public int SendingQueueSize { get; set; }

        /// <summary>
        /// Gets a value indicating whether [log basic session activity like connected and disconnected].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [log basic session activity]; otherwise, <c>false</c>.
        /// </value>
        public bool LogBasicSessionActivity { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether [log all socket exception].
        /// </summary>
        /// <value>
        /// <c>true</c> if [log all socket exception]; otherwise, <c>false</c>.
        /// </value>
        public bool LogAllSocketException { get; set; }

        /// <summary>
        /// Gets/sets the default text encoding.
        /// </summary>
        /// <value>
        /// The text encoding.
        /// </value>
        public string TextEncoding { get; set; }

        /// <summary>
        /// Gets the command assemblies configuration.
        /// </summary>
        /// <value>
        /// The command assemblies.
        /// </value>
        public IEnumerable<ICommandAssemblyConfig> CommandAssemblies { get; set; }

        #endregion
    }
}
