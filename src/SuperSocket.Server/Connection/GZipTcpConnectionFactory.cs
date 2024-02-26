using System;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    public class GZipTcpConnectionFactory<TPackageInfo> : TcpConnectionFactory<TPackageInfo>
    {
        public GZipTcpConnectionFactory(ListenOptions listenOptions, ConnectionOptions connectionOptions, Action<Socket> socketOptionsSetter, IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory)
            : base(listenOptions, connectionOptions, socketOptionsSetter, pipelineFilterFactory)
        {
        }

        public override async Task<IConnection> CreateConnection(object connection)
        {
            var socket = connection as Socket;

            ApplySocketOptions(socket);

            Stream stream = ListenOptions.Security != SslProtocols.None
                ? await GetAuthenticatedSslStream(socket)
                : new NetworkStream(socket);

            var pipeStream = new ReadWriteDelegateStream(
                stream,
                new GZipStream(stream, CompressionMode.Decompress),
                new GZipStream(stream, CompressionMode.Compress));

            return new StreamPipeConnection<TPackageInfo>(pipeStream, socket.RemoteEndPoint, PipelineFilterFactory.Create(socket), ConnectionOptions);
        }
    }
}