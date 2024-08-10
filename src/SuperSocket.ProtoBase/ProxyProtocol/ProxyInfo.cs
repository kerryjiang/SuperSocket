using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    public class ProxyInfo
    {
        public IPAddress SourceIPAddress { get; internal set; }

        public int SourcePort { get; internal set; }

        public IPAddress DestinationIPAddress { get; internal set; }

        public int DestinationPort { get; internal set; }

        public ProxyCommand Command { get; internal set; }

        public int Version { get; internal set; }

        public AddressFamily AddressFamily { get; internal set; }

        public ProtocolType ProtocolType { get; internal set; }

        internal int AddressLength { get; set; }

        public EndPoint SourceEndPoint { get; private set; }

        public EndPoint DestinationEndPoint { get; private set; }

        internal void Prepare()
        {
            SourceEndPoint = new IPEndPoint(SourceIPAddress, SourcePort);
            DestinationEndPoint = new IPEndPoint(DestinationIPAddress, DestinationPort);
        }

        public ProxyInfo()
        {
        }
    }
}