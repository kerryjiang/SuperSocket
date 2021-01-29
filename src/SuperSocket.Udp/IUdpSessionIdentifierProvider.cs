using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using System.Net;

namespace SuperSocket.Udp
{
    public interface IUdpSessionIdentifierProvider
    {
        string GetSessionIdentifier(IPEndPoint remoteEndPoint, ArraySegment<byte> data);
    }
}