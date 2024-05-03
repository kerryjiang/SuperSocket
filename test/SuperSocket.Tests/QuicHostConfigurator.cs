using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Connection;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Quic;
using SuperSocket.Quic.Connection;

namespace SuperSocket.Tests
{
#if NET7_0_OR_GREATER

    public class QuicHostConfigurator : IHostConfigurator
    {
        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        public string WebSocketSchema => "ws";

        public bool IsSecure => false;

        public ListenOptions Listener { get; private set; }

        public IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter,
            ConnectionOptions options) where TPackageInfo : class
        {
            return new QuicClient<TPackageInfo>(pipelineFilter, options);
        }

        private static Random _rd = new Random();

        public void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            hostBuilder
                .UseQuic()
                .ConfigureServices((ctx, services) =>
                    {
                        services.Configure<ServerOptions>((options) =>
                        {
                            var listener = options.Listeners[0];
                            listener.CertificateOptions = new CertificateOptions
                            {
                                FilePath = "supersocket.pfx",
                                Password = "supersocket"
                            };
                            Listener = listener;
                        });
                    }
                );
        }

        public TextReader GetStreamReader(Stream stream, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Stream> GetClientStream(Socket socket)
        {
            throw new NotImplementedException();
        }

        private IPipelineFilter<TextPackageInfo> GetPipelineFilter()
        {
            return new TerminatorPipelineFilter<TextPackageInfo>(new[] { (byte)'\r', (byte)'\n' })
            {
                Decoder = new UdpPackageDecoder()
            };
        }

        class UdpPackageDecoder : IPackageDecoder<TextPackageInfo>
        {
            public TextPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context)
            {
                return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
            }
        }

        class QuicClient<TReceivePackage> : EasyClient<TReceivePackage>
            where TReceivePackage : class
        {
            public QuicClient(IPipelineFilter<TReceivePackage> pipelineFilter, ConnectionOptions options)
                : base(pipelineFilter, options)
            {
            }

            protected override async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint,
                CancellationToken cancellationToken)
            {
#pragma warning disable CA2252
                var quicConnection = await QuicConnection.ConnectAsync(
                    cancellationToken: cancellationToken,
                    options: new QuicClientConnectionOptions
                    {
                        DefaultCloseErrorCode = 0,
                        DefaultStreamErrorCode = 0,
                        RemoteEndPoint = remoteEndPoint,
                        LocalEndPoint = LocalEndPoint,
                        ClientAuthenticationOptions = new SslClientAuthenticationOptions
                        {
                            ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
                            RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                            {
                                return true;
                            }
                        }
                    });

                if (cancellationToken.IsCancellationRequested)
                {
                    OnError($"The connection to {remoteEndPoint} was cancelled.");
                    return false;
                }

                var quicStream =
                    await quicConnection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken);

                var connection = new QuicPipeConnection(quicStream, quicConnection.RemoteEndPoint,
                    quicConnection.LocalEndPoint, Options);

                SetupConnection(connection);

                return true;
            }
        }

        public async ValueTask KeepSequence()
        {
            await Task.Delay(200);
        }

        public Socket CreateClient()
        {
            throw new NotImplementedException();
        }
    }

#endif
}