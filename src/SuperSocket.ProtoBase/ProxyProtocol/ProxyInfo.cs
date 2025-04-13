using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    /// <summary>
    /// Represents information about a proxy connection.
    /// </summary>
    public class ProxyInfo
    {
        /// <summary>
        /// Gets the source IP address of the proxy connection.
        /// </summary>
        public IPAddress SourceIPAddress { get; internal set; }

        /// <summary>
        /// Gets the source port of the proxy connection.
        /// </summary>
        public int SourcePort { get; internal set; }

        /// <summary>
        /// Gets the destination IP address of the proxy connection.
        /// </summary>
        public IPAddress DestinationIPAddress { get; internal set; }

        /// <summary>
        /// Gets the destination port of the proxy connection.
        /// </summary>
        public int DestinationPort { get; internal set; }

        /// <summary>
        /// Gets the proxy command associated with the connection.
        /// </summary>
        public ProxyCommand Command { get; internal set; }

        /// <summary>
        /// Gets the version of the proxy protocol.
        /// </summary>
        public int Version { get; internal set; }

        /// <summary>
        /// Gets the address family of the proxy connection.
        /// </summary>
        public AddressFamily AddressFamily { get; internal set; }

        /// <summary>
        /// Gets the protocol type of the proxy connection.
        /// </summary>
        public ProtocolType ProtocolType { get; internal set; }

        internal int AddressLength { get; set; }
        
        /// <summary>
        /// Gets the source endpoint of the proxy connection.
        /// </summary>
        public EndPoint SourceEndPoint { get; private set; }

        /// <summary>
        /// Gets the destination endpoint of the proxy connection.
        /// </summary>
        public EndPoint DestinationEndPoint { get; private set; }

        /// <summary>
        /// Prepares the source and destination endpoints based on the IP address and port information.
        /// </summary>
        internal void Prepare()
        {
            SourceEndPoint = new IPEndPoint(SourceIPAddress, SourcePort);
            DestinationEndPoint = new IPEndPoint(DestinationIPAddress, DestinationPort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyInfo"/> class.
        /// </summary>
        public ProxyInfo()
        {
        }
    }
}