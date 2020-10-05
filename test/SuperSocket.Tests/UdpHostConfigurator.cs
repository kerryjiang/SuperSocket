using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Udp;

namespace SuperSocket.Tests
{
    public class UdpHostConfigurator : IHostConfigurator
    {
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        
        public string WebSocketSchema => "ws";

        public bool IsSecure => false;

        public ListenOptions Listener { get; private set; }

        public void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            hostBuilder.UseUdp();
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];
                    Listener = listener;
                });
            });
        }

        public IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IEasyClient<TPackageInfo> client) where TPackageInfo : class
        {
            return client;
        }

        public Socket CreateClient()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private async Task UdpReceive(Socket socket, IVirtualChannel channel)
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var buffer = _bufferPool.Rent(1024 * 5);

                try
                {
                    var result = await socket
                        .ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, remoteEndPoint)
                        .ConfigureAwait(false);

                    await channel.WritePipeDataAsync((new ArraySegment<byte>(buffer, 0, buffer.Length)).AsMemory(), CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    break;
                }
                finally
                {
                    _bufferPool.Return(buffer);
                }
            }
        }

        public ValueTask<Stream> GetClientStream(Socket socket)
        {
            var channel = new UdpPipeChannel<TextPackageInfo>(socket, new TerminatorPipelineFilter<TextPackageInfo>(new[] { (byte)'\r', (byte)'\n'}),
                new ChannelOptions(), Listener.GetListenEndPoint());

            UdpReceive(socket, channel).DoNotAwait();

            return new ValueTask<Stream>(new UdpChannelStream(channel));
        }

        public TextReader GetStreamReader(Stream stream, Encoding encoding)
        {
            var channel = (stream as UdpChannelStream).Channel;
            return new UdpTextReader(channel);
        }
    }
}