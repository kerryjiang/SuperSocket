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
    public class TcpSocketListener : IListener
    {
        public ListenOptions Options { get; private set; }
        public bool Start()
        {
            throw new NotImplementedException();
        }

        public event NewClientAcceptHandler NewClientAccepted;
        
        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}