using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// Specifies the types of proxy servers supported.
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// HTTP proxy.
        /// </summary>
        Http,

        /// <summary>
        /// SOCKS4 proxy.
        /// </summary>
        Socks4,

        /// <summary>
        /// SOCKS4a proxy.
        /// </summary>
        Socks4a,

        /// <summary>
        /// SOCKS5 proxy.
        /// </summary>
        Socks5
    }
}