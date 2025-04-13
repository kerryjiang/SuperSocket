using System;
using System.Net;

namespace SuperSocket.Udp
{
    /// <summary>
    /// Provides session identifiers for UDP sessions based on IP addresses.
    /// </summary>
    class IPAddressUdpSessionIdentifierProvider : IUdpSessionIdentifierProvider
    {
        /// <summary>
        /// Gets the session identifier for a UDP session.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint of the UDP session.</param>
        /// <param name="data">The data received from the remote endpoint.</param>
        /// <returns>The session identifier for the UDP session.</returns>
        public string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data)
        {
            return remoteEndPoint.Address.ToString() + ":" + remoteEndPoint.Port;
        }
    }
}