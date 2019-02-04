using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class TcpSocketListenerFactory : IListenerFactory
    {
        public IListener CreateListener(ListenOptions options)
        {
            throw new NotImplementedException();
        }
    }
}