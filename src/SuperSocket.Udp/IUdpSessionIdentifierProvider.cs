using System;
using System.Net;

namespace SuperSocket.Udp
{
    /// <summary>
    /// Provides a mechanism to identify UDP sessions based on remote endpoint and data.
    /// </summary>
    public interface IUdpSessionIdentifierProvider
    {
        /// <summary>
        /// Gets the session identifier for a UDP session.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint of the UDP session.</param>
        /// <param name="data">The data received from the remote endpoint.</param>
        /// <returns>The session identifier for the UDP session.</returns>
        string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data);
    }
}