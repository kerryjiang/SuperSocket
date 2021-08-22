using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using System.IO.Compression;
using System.IO;

namespace SuperSocket.Server
{
    public class TcpChannelCreatorFactory : IChannelCreatorFactory
    {
        private Action<Socket> _socketOptionsSetter;

        public TcpChannelCreatorFactory(IServiceProvider serviceProvider)
        {
            _socketOptionsSetter = serviceProvider.GetService<SocketOptionsSetter>()?.Setter;
        }

        protected virtual void ApplySocketOptions(Socket socket, ListenOptions listenOptions, ChannelOptions channelOptions, ILogger logger)
        {
            try
            {
                if (listenOptions.NoDelay)
                    socket.NoDelay = true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to set NoDelay for the socket.");
            }

            try
            {
                if (channelOptions.ReceiveBufferSize > 0)
                    socket.ReceiveBufferSize = channelOptions.ReceiveBufferSize;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to set ReceiveBufferSize for the socket.");
            }

            try
            {
                if (channelOptions.SendBufferSize > 0)
                    socket.SendBufferSize = channelOptions.SendBufferSize;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to set SendBufferSize for the socket.");
            }

            try
            {
                if (channelOptions.ReceiveTimeout > 0)
                    socket.ReceiveTimeout = channelOptions.ReceiveTimeout;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to set ReceiveTimeout for the socket.");
            }

            try
            {
                if (channelOptions.SendTimeout > 0)
                    socket.SendTimeout = channelOptions.SendTimeout;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to set SendTimeout for the socket.");
            }

            try
            {
                _socketOptionsSetter?.Invoke(socket);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to run socketOptionSetter for the socket.");
            }
        }

        public IChannelCreator CreateChannelCreator<TPackageInfo>(ListenOptions options, ChannelOptions channelOptions, ILoggerFactory loggerFactory, object pipelineFilterFactory)
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

                    var sslStream = new SslStream(stream, true);
                    await sslStream.AuthenticateAsServerAsync(authOptions, CancellationToken.None).ConfigureAwait(false);
                    stream = sslStream;
                }
                if (options.GZipEnable)
                {
                    stream = new GZipReadWriteStream(stream, true);
                }
                return new StreamPipeChannel<TPackageInfo>(stream, s.RemoteEndPoint, s.LocalEndPoint, filterFactory.Create(s), channelOptions);
            });
            return new TcpChannelCreator(options, channelFactory, channelFactoryLogger);
        }
    }
}