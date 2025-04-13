using System;
using System.Net;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents the options for configuring a listener.
    /// </summary>
    public class ListenOptions
    {
        /// <summary>
        /// Gets or sets the IP address to listen on.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the port to listen on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the path for the listener.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the backlog size for the listener.
        /// </summary>
        public int BackLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the NoDelay option is enabled.
        /// </summary>
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets the authentication options for the listener.
        /// </summary>
        public ServerAuthenticationOptions AuthenticationOptions { get; set; }

        /// <summary>
        /// Gets or sets the timeout for accepting connections.
        /// </summary>
        public TimeSpan ConnectionAcceptTimeOut { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets a value indicating whether UDP exclusive address use is enabled.
        /// </summary>
        public bool UdpExclusiveAddressUse { get; set; } = true;

        /// <summary>
        /// Converts the listener options to an <see cref="IPEndPoint"/>.
        /// </summary>
        /// <returns>An <see cref="IPEndPoint"/> representing the listener options.</returns>
        public IPEndPoint ToEndPoint()
        {
            var ip = this.Ip;
            var port = this.Port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Any;
            }
            else if ("IpV6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.IPv6Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            return new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Returns a string representation of the listener options.
        /// </summary>
        /// <returns>A string representation of the listener options.</returns>
        public override string ToString()
        {
            return $"{nameof(Ip)}={Ip}, {nameof(Port)}={Port}, {nameof(AuthenticationOptions)}={AuthenticationOptions}, {nameof(Path)}={Path}, {nameof(BackLog)}={BackLog}, {nameof(NoDelay)}={NoDelay}";
        }
    }
}