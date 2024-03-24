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

namespace SuperSocket.Server.Abstractions.Connections
{
    public abstract class TcpConnectionFactoryBase : IConnectionFactory
    {
        protected ListenOptions ListenOptions { get; }

        protected ConnectionOptions ConnectionOptions { get; }

        protected Action<Socket> SocketOptionsSetter { get; }

        protected ILogger Logger { get; }

        protected IEnumerable<IConnectionStreamInitializer> ConnectionStreamInitializers { get; }

        public TcpConnectionFactoryBase(
            ListenOptions listenOptions,
            ConnectionOptions connectionOptions,
            Action<Socket> socketOptionsSetter,
            IConnectionStreamInitializersFactory connectionStreamInitializersFactory)
        {
            ListenOptions = listenOptions;
            ConnectionOptions = connectionOptions;
            SocketOptionsSetter = socketOptionsSetter;
            Logger = connectionOptions.Logger;

            ConnectionStreamInitializers = connectionStreamInitializersFactory?.Create(listenOptions);
        }

        public abstract Task<IConnection> CreateConnection(object connection, CancellationToken cancellationToken);

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