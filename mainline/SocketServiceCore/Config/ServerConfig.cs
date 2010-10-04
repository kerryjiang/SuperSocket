using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Config
{
    public class ServerConfig : IServerConfig
    {
        #region IServerConfig Members

        public string ServiceName { get; set; }

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

        public bool LogCommand { get; set; }

        public bool ClearIdleSession { get; set; }

        public int ClearIdleSessionInterval { get; set; }

        public int IdleSessionTimeOut { get; set; }

        public ICertificateConfig Certificate { get; set; }

        #endregion
    }
}
