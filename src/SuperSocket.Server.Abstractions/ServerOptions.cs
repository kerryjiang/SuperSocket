using System.Collections.Generic;
using System.Text;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions
{
    public class ServerOptions : ConnectionOptions
    {
        public string Name { get; set; }

        public List<ListenOptions> Listeners { get; set; }

        public Encoding DefaultTextEncoding { get; set; }

        public int ClearIdleSessionInterval { get; set; } = 120;

        public int IdleSessionTimeOut { get; set; } = 300;

        /// <summary>
        /// In seconds.
        /// </summary>
        public int PackageHandlingTimeOut { get; set; } = 30;

        public bool EnableProxyProtocol { get; set; }
    }
}