using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.Collections.Specialized;

namespace SuperSocket.SocketEngine.Configuration
{
    public class Server : ConfigurationElementBase, IServerConfig
    {
        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get { return this["serviceName"] as string; }
        }

        [ConfigurationProperty("protocol", IsRequired = false)]
        public string Protocol
        {
            get { return this["protocol"] as string; }
        }

        [ConfigurationProperty("ip", IsRequired = false)]
        public string Ip
        {
            get { return this["ip"] as string; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        [ConfigurationProperty("mode", IsRequired = false, DefaultValue = "Sync")]
        public SocketMode Mode
        {
            get { return (SocketMode)this["mode"]; }
        }

        [ConfigurationProperty("disabled", DefaultValue = "false")]
        public bool Disabled
        {
            get { return (bool)this["disabled"]; }
        }

        [ConfigurationProperty("enableManagementService", DefaultValue = "false")]
        public bool EnableManagementService
        {
            get { return (bool)this["enableManagementService"]; }
        }

        [ConfigurationProperty("provider", IsRequired = false)]
        public string Provider
        {
            get { return (string)this["provider"]; }
        }

        [ConfigurationProperty("readTimeOut", IsRequired = false, DefaultValue = 0)]
        public int ReadTimeOut
        {
            get { return (int)this["readTimeOut"]; }
        }

        [ConfigurationProperty("sendTimeOut", IsRequired = false, DefaultValue = 0)]
        public int SendTimeOut
        {
            get { return (int)this["sendTimeOut"]; }
        }

        [ConfigurationProperty("maxConnectionNumber", IsRequired = false, DefaultValue = 100)]
        public int MaxConnectionNumber
        {
            get { return (int)this["maxConnectionNumber"]; }
        }

        [ConfigurationProperty("receiveBufferSize", IsRequired = false, DefaultValue = 2048)]
        public int ReceiveBufferSize
        {
            get { return (int)this["receiveBufferSize"]; }
        }

        [ConfigurationProperty("sendBufferSize", IsRequired = false, DefaultValue = 2048)]
        public int SendBufferSize
        {
            get { return (int)this["sendBufferSize"]; }
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

        public ICertificateConfig Certificate
        {
            get { return CertificateConfig; }
        }

        [ConfigurationProperty("security", IsRequired = false, DefaultValue = "None")]
        public string Security
        {
            get
            {
                return (string)this["security"];
            }
        }

        /// <summary>
        /// Gets the max command length in bytes. Default value is 1024.
        /// </summary>
        /// <value>
        /// The length of the max command.
        /// </value>
        [ConfigurationProperty("maxCommandLength", IsRequired = false, DefaultValue = 1024)]
        public int MaxCommandLength
        {
            get
            {
                return (int)this["maxCommandLength"];
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
        
        [ConfigurationProperty("connectionFilters", IsRequired = false)]
        public string ConnectionFilters
        {
            get
            {
                return (string)this["connectionFilters"];
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
        /// Gets a value indicating whether [enable dynamic command](support commands written in IronPython).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [dynamic command is enabled]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("enableDynamicCommand", IsRequired = false, DefaultValue = false)]
        public bool EnableDynamicCommand
        {
            get
            {
                return (bool)this["enableDynamicCommand"];
            }
        }
    }
}
