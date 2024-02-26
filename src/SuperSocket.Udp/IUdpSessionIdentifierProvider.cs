using System;
using System.Net;

namespace SuperSocket.Udp
{
    public interface IUdpSessionIdentifierProvider
    {
        string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data);
    }
}