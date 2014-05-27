using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using System.Linq;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Server configuration
    /// </summary>
    public partial class Server : ConfigurationElementBase, IServerConfig
    {
        /// <summary>
        /// Gets the name of the server type this appServer want to use.
        /// </summary>
        /// <value>
        /// The name of the server type.
        /// </value>
        [ConfigurationProperty("serverTypeName", IsRequired = false)]
        public string ServerTypeName
        {
            get { return this["serverTypeName"] as string; }
        }

        /// <summary>
        /// Gets the type definition of the appserver.
        /// </summary>
        /// <value>
        /// The type of the server.
        /// </value>
        [ConfigurationProperty("serverType", IsRequired = false)]
        public string ServerType
        {
            get { return this["serverType"] as string; }
        }

        /// <summary>
        /// Gets the Receive filter factory.
        /// </summary>
        [ConfigurationProperty("receiveFilterFactory", IsRequired = false)]
        public string ReceiveFilterFactory
        {
            get { return this["receiveFilterFactory"] as string; }
        }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        [ConfigurationProperty("ip", IsRequired = false)]
        public string Ip
        {
            get { return this["ip"] as string; }
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = false)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// Gets the mode.
        /// </summary>
        [ConfigurationProperty("mode", IsRequired = false, DefaultValue = "Tcp")]
        public SocketMode Mode
        {
            get { return (SocketMode)this["mode"]; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IServerConfig"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("disabled", DefaultValue = "false")]
        public bool Disabled
        {
            get { return (bool)this["disabled"]; }
        }

        /// <summary>
        /// Gets the send time out.
        /// </summary>
        [ConfigurationProperty("sendTimeOut", IsRequired = false, DefaultValue = ServerConfig.DefaultSendTimeout)]
        public int SendTimeOut
        {
            get { return (int)this["sendTimeOut"]; }
        }

        /// <summary>
        /// Gets the max connection number.
        /// </summary>
        [ConfigurationProperty("maxConnectionNumber", IsRequired = false, DefaultValue = ServerConfig.DefaultMaxConnectionNumber)]
        public int MaxConnectionNumber
        {
            get { return (int)this["maxConnectionNumber"]; }
        }

        /// <summary>
        /// Gets the size of the receive buffer.
        /// </summary>
        /// <value>
        /// The size of the receive buffer.
        /// </value>
        [ConfigurationProperty("receiveBufferSize", IsRequired = false, DefaultValue = ServerConfig.DefaultReceiveBufferSize)]
        public int ReceiveBufferSize
        {
            get { return (int)this["receiveBufferSize"]; }
        }

        /// <summary>
        /// Gets the size of the send buffer.
        /// </summary>
        /// <value>
        /// The size of the send buffer.
        /// </value>
        [ConfigurationProperty("sendBufferSize", IsRequired = false, DefaultValue = ServerConfig.DefaultSendBufferSize)]
        public int SendBufferSize
        {
            get { return (int)this["sendBufferSize"]; }
        }

        /// <summary>
        /// Gets a value indicating whether sending is in synchronous mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sync send]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("syncSend", IsRequired = false, DefaultValue = false)]
        public bool SyncSend
        {
            get { return (bool)this["syncSend"]; }
        }

        /// <summary>
        /// Gets a value indicating whether log command in log file.
        /// </summary>
        /// <value><c>true</c> if log command; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("logCommand", IsRequired = false, DefaultValue = false)]
        public bool LogCommand
        {
            get { return (bool)this["logCommand"]; }
        }


        /// <summary>
        /// Gets a value indicating whether [log basic session activity like connected and disconnected].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [log basic session activity]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("logBasicSessionActivity", IsRequired = false, DefaultValue = true)]
        public bool LogBasicSessionActivity
        {
            get { return (bool)this["logBasicSessionActivity"]; }
        }

        /// <summary>
        /// Gets a value indicating whether [log all socket exception].
        /// </summary>
        /// <value>
        /// <c>true</c> if [log all socket exception]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("logAllSocketException", IsRequired = false, DefaultValue = false)]
        public bool LogAllSocketException
        {
            get { return (bool)this["logAllSocketException"]; }
        }

        /// <summary>
        /// Gets a value indicating whether clear idle session.
        /// </summary>
        /// <value><c>true</c> if clear idle session; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("clearIdleSession", IsRequired = false, DefaultValue = false)]
        public bool ClearIdleSession
        {
            get { return (bool)this["clearIdleSession"]; }
        }

        /// <summary>
        /// Gets the clear idle session interval, in seconds.
        /// </summary>
        /// <value>The clear idle session interval.</value>
        [ConfigurationProperty("clearIdleSessionInterval", IsRequired = false, DefaultValue = ServerConfig.DefaultClearIdleSessionInterval)]
        public int ClearIdleSessionInterval
        {
            get { return (int)this["clearIdleSessionInterval"]; }
        }


        /// <summary>
        /// Gets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>The idle session time out.</value>
        [ConfigurationProperty("idleSessionTimeOut", IsRequired = false, DefaultValue = ServerConfig.DefaultIdleSessionTimeOut)]
        public int IdleSessionTimeOut
        {
            get { return (int)this["idleSessionTimeOut"]; }
        }

        /// <summary>
        /// Gets the certificate config.
        /// </summary>
        /// <value>The certificate config.</value>
        [ConfigurationProperty("certificate", IsRequired = false)]
        public CertificateConfig CertificateConfig
        {
            get
            {
                return (CertificateConfig)this["certificate"];
            }
        }

        /// <summary>
        /// Gets X509Certificate configuration.
        /// </summary>
        /// <value>
        /// X509Certificate configuration.
        /// </value>
        public ICertificateConfig Certificate
        {
            get { return CertificateConfig; }
        }

        /// <summary>
        /// Gets the security protocol, X509 certificate.
        /// </summary>
        [ConfigurationProperty("security", IsRequired = false, DefaultValue = "None")]
        public string Security
        {
            get
            {
                return (string)this["security"];
            }
        }

        /// <summary>
        /// Gets the max allowed length of request.
        /// </summary>
        /// <value>
        /// The max allowed length of request.
        /// </value>
        [ConfigurationProperty("maxRequestLength", IsRequired = false, DefaultValue = ServerConfig.DefaultMaxRequestLength)]
        public int MaxRequestLength
        {
            get
            {
                return (int)this["maxRequestLength"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether [disable session snapshot]
        /// </summary>
        [ConfigurationProperty("disableSessionSnapshot", IsRequired = false, DefaultValue = false)]
        public bool DisableSessionSnapshot
        {
            get
            {
                return (bool)this["disableSessionSnapshot"];
            }
        }

        /// <summary>
        /// Gets the interval to taking snapshot for all live sessions.
        /// </summary>
        [ConfigurationProperty("sessionSnapshotInterval", IsRequired = false, DefaultValue = ServerConfig.DefaultSessionSnapshotInterval)]
        public int SessionSnapshotInterval
        {
            get
            {
                return (int)this["sessionSnapshotInterval"];
            }
        }

        /// <summary>
        /// Gets the connection filters used by this server instance.
        /// </summary>
        /// <value>
        /// The connection filters's name list, seperated by comma
        /// </value>
        [ConfigurationProperty("connectionFilter", IsRequired = false)]
        public string ConnectionFilter
        {
            get
            {
                return (string)this["connectionFilter"];
            }
        }

        /// <summary>
        /// Gets the command loader, multiple values should be separated by comma.
        /// </summary>
        [ConfigurationProperty("commandLoader", IsRequired = false)]
        public string CommandLoader
        {
            get
            {
                return (string)this["commandLoader"];
            }
        }

        /// <summary>
        /// Gets the start keep alive time, in seconds
        /// </summary>
        [ConfigurationProperty("keepAliveTime", IsRequired = false, DefaultValue = ServerConfig.DefaultKeepAliveTime)]
        public int KeepAliveTime
        {
            get
            {
                return (int)this["keepAliveTime"];
            }
        }

        /// <summary>
        /// Gets the keep alive interval, in seconds.
        /// </summary>
        [ConfigurationProperty("keepAliveInterval", IsRequired = false, DefaultValue = ServerConfig.DefaultKeepAliveInterval)]
        public int KeepAliveInterval
        {
            get
            {
                return (int)this["keepAliveInterval"];
            }
        }

        /// <summary>
        /// Gets the backlog size of socket listening.
        /// </summary>
        [ConfigurationProperty("listenBacklog", IsRequired = false, DefaultValue = ServerConfig.DefaultListenBacklog)]
        public int ListenBacklog
        {
            get
            {
                return (int)this["listenBacklog"];
            }
        }

        /// <summary>
        /// Gets the startup order of the server instance.
        /// </summary>
        [ConfigurationProperty("startupOrder", IsRequired = false, DefaultValue = 0)]
        public int StartupOrder
        {
            get
            {
                return (int)this["startupOrder"];
            }
        }

        /// <summary>
        /// Gets/sets the size of the sending queue.
        /// </summary>
        /// <value>
        /// The size of the sending queue.
        /// </value>
        [ConfigurationProperty("sendingQueueSize", IsRequired = false, DefaultValue = ServerConfig.DefaultSendingQueueSize)]
        public int SendingQueueSize
        {
            get
            {
                return (int)this["sendingQueueSize"];
            }
        }

        /// <summary>
        /// Gets the logfactory name of the server instance.
        /// </summary>
        [ConfigurationProperty("logFactory", IsRequired = false, DefaultValue = "")]
        public string LogFactory
        {
            get
            {
                return (string)this["logFactory"];
            }
        }

        /// <summary>
        /// Gets the default text encoding.
        /// </summary>
        /// <value>
        /// The text encoding.
        /// </value>
        [ConfigurationProperty("textEncoding", IsRequired = false, DefaultValue = "")]
        public string TextEncoding
        {
            get
            {
                return (string)this["textEncoding"];
            }
        }

        /// <summary>
        /// Gets the listeners' configuration.
        /// </summary>
        [ConfigurationProperty("listeners", IsRequired = false)]
        public ListenerConfigCollection Listeners
        {
            get
            {
                return this["listeners"] as ListenerConfigCollection;
            }
        }

        /// <summary>
        /// Gets the listeners' configuration.
        /// </summary>
        IEnumerable<IListenerConfig> IServerConfig.Listeners
        {
            get
            {
                return this.Listeners;
            }
        }

        /// <summary>
        /// Gets the command assemblies configuration.
        /// </summary>
        /// <value>
        /// The command assemblies.
        /// </value>
        [ConfigurationProperty("commandAssemblies", IsRequired = false)]
        public CommandAssemblyCollection CommandAssemblies
        {
            get
            {
                return this["commandAssemblies"] as CommandAssemblyCollection;
            }
        }

        IEnumerable<ICommandAssemblyConfig> IServerConfig.CommandAssemblies
        {
            get { return this.CommandAssemblies; }
        }

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// To keep compatible with old configuration
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            //To keep compatible with old configuration
            if (!"serviceName".Equals(name, StringComparison.OrdinalIgnoreCase))
                return base.OnDeserializeUnrecognizedAttribute(name, value);

            this["serverTypeName"] = value;
            return true;
        }
    }
}
