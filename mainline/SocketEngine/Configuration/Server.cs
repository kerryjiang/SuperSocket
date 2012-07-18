using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Server configuration
    /// </summary>
    [Serializable]
    public class Server : ConfigurationElementBase, IServerConfig
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get { return this["serviceName"] as string; }
        }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        [ConfigurationProperty("requestFilter", IsRequired = false)]
        public string RequestFilter
        {
            get { return this["requestFilter"] as string; }
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
        [ConfigurationProperty("sendTimeOut", IsRequired = false, DefaultValue = 0)]
        public int SendTimeOut
        {
            get { return (int)this["sendTimeOut"]; }
        }

        /// <summary>
        /// Gets the max connection number.
        /// </summary>
        [ConfigurationProperty("maxConnectionNumber", IsRequired = false, DefaultValue = 100)]
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
        [ConfigurationProperty("receiveBufferSize", IsRequired = false, DefaultValue = 2048)]
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
        [ConfigurationProperty("sendBufferSize", IsRequired = false, DefaultValue = 2048)]
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
        [ConfigurationProperty("clearIdleSessionInterval", IsRequired = false, DefaultValue = 120)]
        public int ClearIdleSessionInterval
        {
            get { return (int)this["clearIdleSessionInterval"]; }
        }


        /// <summary>
        /// Gets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>The idle session time out.</value>
        [ConfigurationProperty("idleSessionTimeOut", IsRequired = false, DefaultValue = 300)]
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
            get { return (CertificateConfig)this["certificate"]; }
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
        [ConfigurationProperty("maxRequestLength", IsRequired = false, DefaultValue = 1024)]
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
        [ConfigurationProperty("sessionSnapshotInterval", IsRequired = false, DefaultValue = 5)]
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
        [ConfigurationProperty("keepAliveTime", IsRequired = false, DefaultValue = 600)]
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
        [ConfigurationProperty("keepAliveInterval", IsRequired = false, DefaultValue = 60)]
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
        [ConfigurationProperty("listenBacklog", IsRequired = false, DefaultValue = 100)]
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
    }
}
