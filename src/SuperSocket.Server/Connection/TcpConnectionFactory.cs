using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        private IEnumerable<IConnectionStreamInitializer> _connectionStreamInitializers;

        public TcpConnectionFactory(
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions,
            Action<Socket> socketOptionsSetter,
            IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory,
            IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            ListenOptions = listenOptions;
            ConnectionOptions = connectionOptions;
            SocketOptionsSetter = socketOptionsSetter;
            PipelineFilterFactory = pipelineFilterFactory;
            Logger = connectionOptions.Logger;

            _connectionStreamInitializers = connectionStreamInitializersFactory.Create(listenOptions);
        }

        public virtual async Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken)
        {
            var socket = connection as Socket;

            ApplySocketOptions(socket);

            if (_connectionStreamInitializers is IEnumerable<IConnectionStreamInitializer> connectionStreamInitializers
                && connectionStreamInitializers.Any())
            {
                var stream = default(Stream);

                foreach (var initializer in connectionStreamInitializers)
                {
                    stream = await initializer.InitializeAsync(socket, stream, cancellationToken);
                }

                return new StreamPipeConnection<TPackageInfo>(stream, socket.RemoteEndPoint, socket.LocalEndPoint, PipelineFilterFactory.Create(socket), ConnectionOptions);
            }

            return new TcpPipeConnection<TPackageInfo>(socket, PipelineFilterFactory.Create(socket), ConnectionOptions);
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