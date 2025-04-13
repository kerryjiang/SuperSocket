using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    /// <summary>
    /// Represents security options for an SSL/TLS client connection.
    /// </summary>
    public class SecurityOptions : SslClientAuthenticationOptions
    {
        /// <summary>
        /// Gets or sets the network credentials used for authentication.
        /// </summary>
        public NetworkCredential Credential { get; set; }
    }
}