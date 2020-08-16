using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SecurityOptions : SslClientAuthenticationOptions
    {
        public NetworkCredential Credential { get; set; }
    }
}