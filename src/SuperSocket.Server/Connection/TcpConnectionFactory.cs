using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class TcpConnectionFactory<TPackageInfo> : IConnectionFactory
    {
        protected ListenOptions ListenOptions { get; }

        protected ConnectionOptions ConnectionOptions { get; }

        protected Action<Socket> SocketOptionsSetter { get; }

        protected IPipelineFilterFactory<TPackageInfo> PipelineFilterFactory;

        protected ILogger Logger { get; }

        public TcpConnectionFactory(ListenOptions listenOptions, ConnectionOptions connectionOptions, Action<Socket> socketOptionsSetter, IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
        {
            ListenOptions = listenOptions;
            ConnectionOptions = connectionOptions;
            SocketOptionsSetter = socketOptionsSetter;
            PipelineFilterFactory = pipelineFilterFactory;
            Logger = connectionOptions.Logger;
        }

        public virtual async Task<IConnection> CreateConnection(object connection)
        {
            var socket = connection as Socket;

            ApplySocketOptions(socket);

            if (ListenOptions.Security != SslProtocols.None)
            {
                return await CreateSecurePipeConnection(socket);
            }
            else
            {
                return await CreatePipeConnection(socket);
            }
        }

        protected virtual Task<IConnection> CreatePipeConnection(Socket socket)
        {
            return Task.FromResult<IConnection>(new TcpPipeConnection<TPackageInfo>(socket, PipelineFilterFactory.Create(socket), ConnectionOptions));
        }

        protected virtual async Task<IConnection> CreateSecurePipeConnection(Socket socket)
        {
            var stream = await GetAuthenticatedSslStream(socket);
            return new SslStreamPipeConnection<TPackageInfo>(stream, socket.RemoteEndPoint, socket.LocalEndPoint, PipelineFilterFactory.Create(socket), ConnectionOptions);
        }

        protected async Task<SslStream> GetAuthenticatedSslStream(Socket socket)
        {
            var authOptions = new SslServerAuthenticationOptions();

            authOptions.EnabledSslProtocols = ListenOptions.Security;
            authOptions.ServerCertificate = ListenOptions.CertificateOptions.Certificate;
            authOptions.ClientCertificateRequired = ListenOptions.CertificateOptions.ClientCertificateRequired;

            if (ListenOptions.CertificateOptions.RemoteCertificateValidationCallback != null)
                authOptions.RemoteCertificateValidationCallback = ListenOptions.CertificateOptions.RemoteCertificateValidationCallback;

            var stream = new SslStream(new NetworkStream(socket, true), false);
            await stream.AuthenticateAsServerAsync(authOptions, CancellationToken.None);
            return stream;
        }

        protected virtual void ApplySocketOptions(Socket socket)
        {
            try
            {
                if (ListenOptions.NoDelay)
                    socket.NoDelay = true;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set NoDelay for the socket.");
            }

            try
            {
                if (ConnectionOptions.ReceiveBufferSize > 0)
                    socket.ReceiveBufferSize = ConnectionOptions.ReceiveBufferSize;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set ReceiveBufferSize for the socket.");
            }

            try
            {
                if (ConnectionOptions.SendBufferSize > 0)
                    socket.SendBufferSize = ConnectionOptions.SendBufferSize;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set SendBufferSize for the socket.");
            }

            try
            {
                if (ConnectionOptions.ReceiveTimeout > 0)
                    socket.ReceiveTimeout = ConnectionOptions.ReceiveTimeout;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set ReceiveTimeout for the socket.");
            }

            try
            {
                if (ConnectionOptions.SendTimeout > 0)
                    socket.SendTimeout = ConnectionOptions.SendTimeout;
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to set SendTimeout for the socket.");
            }

            try
            {
                SocketOptionsSetter?.Invoke(socket);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to run socketOptionSetter for the socket.");
            }
        }
    }
}