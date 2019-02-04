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
    public class AppSession : IAppSession
    {
        internal AppSession(IServerInfo server, IChannelBase channel)
        {
            Server = server;
            Channel = channel;
        }

        public IServerInfo Server { get; internal set; }

        public IChannelBase Channel { get; private set; }
    }
}