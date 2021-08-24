using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.GZip
{
    public class GZipTcpChannelCreatorFactory : TcpChannelCreatorFactory, IChannelCreatorFactory
    {
        public GZipTcpChannelCreatorFactory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public new IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

            var channelFactoryLogger = loggerFactory.CreateLogger(nameof(TcpChannelCreator));

            var channelFactory = new Func<Socket, ValueTask<IChannel>>(async (s) =>
            {
                ApplySocketOptions(s, options, channelOptions, channelFactoryLogger);

                Stream stream = new NetworkStream(s, true);
                if (options.Security != SslProtocols.None)
                {
                    var authOptions = new SslServerAuthenticationOptions();

                    authOptions.EnabledSslProtocols = options.Security;
                    authOptions.ServerCertificate = options.CertificateOptions.Certificate;
                    authOptions.ClientCertificateRequired = options.CertificateOptions.ClientCertificateRequired;

                    if (options.CertificateOptions.RemoteCertificateValidationCallback != null)
                        authOptions.RemoteCertificateValidationCallback = options.CertificateOptions.RemoteCertificateValidationCallback;

                    var sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsServerAsync(authOptions, CancellationToken.None).ConfigureAwait(false);

                    stream = sslStream;
                }
                stream = new GZipReadWriteStream(stream, true);
                return new StreamPipeChannel<TPackageInfo>(stream, s.RemoteEndPoint, s.LocalEndPoint, filterFactory.Create(s), channelOptions);
            });

            return new TcpChannelCreator(options, channelFactory, channelFactoryLogger);
        }
    }
}
