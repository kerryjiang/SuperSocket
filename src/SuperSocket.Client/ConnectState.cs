using System;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    public class ConnectState
    {
        public ConnectState()
        {

        }

        private ConnectState(bool cancelled)
        {
            Cancelled = cancelled;
        }

        public bool Result { get; set; }

        public bool Cancelled { get; private set; }

        public Exception Exception { get; set; }

        public Socket Socket { get; set; }

        public Stream Stream { get; set; }

        public static readonly ConnectState CancelledState = new ConnectState(false);

        public IChannel<TReceivePackage> CreateChannel<TReceivePackage>(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions channelOptions)
            where TReceivePackage : class
        {
            var stream = this.Stream;
            var socket = this.Socket;

            if (stream != null)
            {
                return new StreamPipeChannel<TReceivePackage>(stream , socket.RemoteEndPoint, socket.LocalEndPoint, pipelineFilter, channelOptions);
            }
            else
            {
                return new TcpPipeChannel<TReceivePackage>(socket, pipelineFilter, channelOptions);
            }
        }
    }
}