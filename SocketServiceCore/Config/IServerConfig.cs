using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Config
{
    public interface IServerConfig
    {
        string ServiceName { get; }

        string Ip { get; }

        int Port { get; }

        NameValueConfigurationCollection Parameters { get; }

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
        /// Gets the idle session timeout time length, in minutes.
        /// </summary>
        /// <value>The idle session time out.</value>
        int IdleSessionTimeOut { get; }

        /// <summary>
        /// Gets X509Certificate configuration.
        /// </summary>
        /// <value>X509Certificate configuration.</value>
        ICertificateConfig Certificate { get; }
    }
}
