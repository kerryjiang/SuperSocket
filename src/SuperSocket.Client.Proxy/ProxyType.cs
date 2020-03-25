using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client.Proxy
{
    public enum ProxyType
    {
        Http,
        Socks4,
        Socks4a,
        Socks5
    }
}