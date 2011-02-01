using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Security.Authentication;
using System.Collections.Specialized;

namespace SuperSocket.SocketBase.Config
{
    public interface IServerConfig
    {
        string ServiceName { get; }

        string Protocol { get; }

        string Ip { get; }

        int Port { get; }

        NameValueCollection Options { get; }

        string Provider { get; }

        bool Disabled { get; }

        string Name { get; }

        SocketMode Mode { get; }

        bool EnableManagementService { get; }

        int ReadTimeOut { get; }

        int SendTimeOut { get; }

        int MaxConnectionNumber { get; }

        int ReceiveBufferSize { get; }

        int SendBufferSize { get; }

        /// <summary>
        /// Gets a value indicating whether log command in log file.
        /// </summary>
        /// <value><c>true</c> if log command; otherwise, <c>false</c>.</value>
        bool LogCommand { get; }

        /// <summary>
        /// Gets a value indicating whether clear idle session.
        /// </summary>
        /// <value><c>true</c> if clear idle session; otherwise, <c>false</c>.</value>
        bool ClearIdleSession { get; }

        /// <summary>
        /// Gets the clear idle session interval, in seconds.
        /// </summary>
        /// <value>The clear idle session interval.</value>
        int ClearIdleSessionInterval { get; }


        /// <summary>
        /// Gets the idle session timeout time length, in seconds.
        /// </summary>
        /// <value>The idle session time out.</value>
        int IdleSessionTimeOut { get; }

        /// <summary>
        /// Gets X509Certificate configuration.
        /// </summary>
        /// <value>X509Certificate configuration.</value>
        ICertificateConfig Certificate { get; }


        /// <summary>
        /// Gets the security protocol, X509 certificate.
        /// </summary>
        string Security { get; }


        /// <summary>
        /// Gets the max command length.
        /// </summary>
        /// <value>
        /// The length of the max command.
        /// </value>
        int MaxCommandLength { get; }


        /// <summary>
        /// Gets the interval to taking snapshot for all live sessions.
        /// </summary>
        int SessionSnapshotInterval { get; }
        
        /// <summary>
        /// Gets the connection filters used by this server instance.
        /// </summary>
        /// <value>
        /// The connection filters's name list, seperated by comma
        /// </value>
        string ConnectionFilters { get; }
    }
}
