using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using System.IO.Compression;
using System.Linq;


namespace SuperSocket.Client
{
    public abstract class EasyClient : IEasyClient
    {

        protected IConnection Connection { get; private set; }

        public IConnector Proxy { get; set; }

        protected ILogger Logger { get; set; }

        protected ConnectionOptions Options { get; private set; }

        public IPEndPoint LocalEndPoint { get; set; }

        public SecurityOptions Security { get; set; }

        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;

        public static int? SocketSenderPoolSzie { get; set; }

        internal static readonly int DefaultSocketSenderPoolSzie = 10;

        protected EasyClient()
            : this(NullLogger.Instance)
        {
            
        }

        protected EasyClient(ILogger logger)
            : this(new ConnectionOptions { Logger = logger })
        {
        }

        public EasyClient(ConnectionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;
            Logger = options.Logger;
        }

        protected virtual IConnector GetConnector()
        {
            var connectors = new List<IConnector>();

            if (Proxy is IConnector proxy)
            {
                connectors.Add(proxy);
            }
            else
            {
                connectors.Add(new SocketConnector(LocalEndPoint));
            }

            var security = Security;

            if (security != null)
            {
                if (security.EnabledSslProtocols != SslProtocols.None)
                    connectors.Add(new SslStreamConnector(security));
            }

            if (CompressionLevel != CompressionLevel.NoCompression)
            {
                connectors.Add(new GZipConnector(CompressionLevel));
            }
            
            return BuildConnectors(connectors);
        }

        protected IConnector BuildConnectors(IEnumerable<IConnector> connectors)
        {
            var prevConnector = default(ConnectorBase);

            foreach (var connector in connectors)
            {
                if (prevConnector != null)
                {
                    prevConnector.NextConnector = connector;
                }

                prevConnector = connector as ConnectorBase;
            }
            
            return connectors.First();
        }

        ValueTask<bool> IEasyClient.ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return ConnectAsync(remoteEndPoint, cancellationToken);
        }

        protected virtual async ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            var connector = GetConnector();
            var state = await connector.ConnectAsync(remoteEndPoint, null, cancellationToken);

            if (state.Cancelled || cancellationToken.IsCancellationRequested)
            {
                OnError($"The connection to {remoteEndPoint} was cancelled.", state.Exception);
                return false;
            }                

            if (!state.Result)
            {
                OnError($"Failed to connect to {remoteEndPoint}", state.Exception);
                return false;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket is null.");

            SetupConnection(state.CreateConnection(Options));
            return true;
        }

        public void AsUdp(IPEndPoint remoteEndPoint, ArrayPool<byte> bufferPool = null, int bufferSize = 4096)
        { 
            var localEndPoint = LocalEndPoint;

            if (localEndPoint == null)
            {
                localEndPoint = new IPEndPoint(remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            }

            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            // bind the local endpoint
            socket.Bind(localEndPoint);

            var connection = new UdpPipeConnection(socket, this.Options, remoteEndPoint);

            SetupConnection(connection);

            UdpReceive(socket, connection, bufferPool, bufferSize);
        }

        private async void UdpReceive(Socket socket, UdpPipeConnection connection, ArrayPool<byte> bufferPool, int bufferSize)
        {
            if (bufferPool == null)
                bufferPool = ArrayPool<byte>.Shared;

            while (true)
            {
                var buffer = bufferPool.Rent(bufferSize);

                try
                {
                    var result = await socket
                        .ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, connection.RemoteEndPoint)
                        .ConfigureAwait(false);

                    await connection.WritePipeDataAsync((new ArraySegment<byte>(buffer, 0, result.ReceivedBytes)).AsMemory(), CancellationToken.None);
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    OnError($"Failed to receive UDP data.", e);
                }
                finally
                {
                    bufferPool.Return(buffer);
                }
            }
        }

        protected virtual void SetupConnection(IConnection connection)
        {
            connection.Closed += OnConnectionClosed;
            Connection = connection;
        }

        void IEasyClient.StartReceive()
        {
            StartReceive();
        }

        /// <summary>
        /// Start receive packages and handle the packages by event handler
        /// </summary>
        private void StartReceive()
        {
            StartReceiveAsync().ContinueWith((task, state) =>
            {
                var client = (EasyClient)state;

                if (task.IsFaulted)
                {
                    client.OnError("Failed to start receive.", task.Exception);
                    return;
                }

                if (task.IsCanceled)
                {
                    client.OnError("The receive task was cancelled.");
                    return;
                }
            }, this, TaskContinuationOptions.OnlyOnFaulted);;
        }

        protected abstract Task StartReceiveAsync();

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            Connection.Closed -= OnConnectionClosed;
            OnClosed(this, e);
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            var handler = Closed;

            if (handler != null)
            {
                if (Interlocked.CompareExchange(ref Closed, null, handler) == handler)
                {
                    handler.Invoke(sender, e);
                }
            }
        }

        protected virtual void OnError(string message, Exception exception)
        {
            Logger?.LogError(exception, message);
        }

        protected virtual void OnError(string message)
        {
            Logger?.LogError(message);
        }

        ValueTask IEasyClient.SendAsync(ReadOnlyMemory<byte> data)
        {
            return SendAsync(data);
        }

        protected virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            await Connection.SendAsync(data);
        }

        ValueTask IEasyClient.SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            return SendAsync<TSendPackage>(packageEncoder, package);
        }

        protected virtual async ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package)
        {
            await Connection.SendAsync(packageEncoder, package);
        }

        public event EventHandler Closed;

        public virtual async ValueTask CloseAsync()
        {
            await Connection.CloseAsync(CloseReason.LocalClosing);
            OnClosed(this, EventArgs.Empty);
        }
    }

    public class EasyClient<TReceivePackage> : EasyClient, IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        private IPipelineFilter<TReceivePackage> _pipelineFilter;

        IAsyncEnumerator<TReceivePackage> _packageStream;

        public event PackageHandler<TReceivePackage> PackageHandler;

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter)
            : this(pipelineFilter, NullLogger.Instance)
        {

        }

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger)
            : this(pipelineFilter, new ConnectionOptions { Logger = logger })
        {

        }

        public EasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ConnectionOptions options)
            : base(options)
        {
            if (pipelineFilter == null)
                throw new ArgumentNullException(nameof(pipelineFilter));

            _pipelineFilter = pipelineFilter;
        }

        public virtual IEasyClient<TReceivePackage> AsClient()
        {
            return this;
        }

        protected override void SetupConnection(IConnection connection)
        {
            base.SetupConnection(connection);
            _packageStream = connection.GetPackageStream(_pipelineFilter);
        }

        ValueTask<TReceivePackage> IEasyClient<TReceivePackage>.ReceiveAsync()
        {
            return ReceiveAsync();
        }

        /// <summary>
        /// Try to receive one package
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<TReceivePackage> ReceiveAsync()
        {
            var p = await _packageStream.ReceiveAsync();

            if (p != null)
                return p;

            OnClosed(Connection, EventArgs.Empty);
            return null;
        }

        protected override async Task StartReceiveAsync()
        {
            var enumerator = _packageStream;

            while (await enumerator.MoveNextAsync())
            {
                await OnPackageReceived(enumerator.Current);
            }
        }

        protected virtual async ValueTask OnPackageReceived(TReceivePackage package)
        {
            var handler = PackageHandler;

            try
            {
                await handler.Invoke(this, package);
            }
            catch (Exception e)
            {
                OnError("Unhandled exception happened in PackageHandler.", e);
            }
        }
    }

    public class EasyClient<TPackage, TSendPackage> : EasyClient<TPackage>, IEasyClient<TPackage, TSendPackage>
        where TPackage : class
    {
        private IPackageEncoder<TSendPackage> _packageEncoder;

        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ILogger logger = null)
            : this(pipelineFilter, packageEncoder, new ConnectionOptions { Logger = logger })
        {

        }

        public EasyClient(IPipelineFilter<TPackage> pipelineFilter, IPackageEncoder<TSendPackage> packageEncoder, ConnectionOptions options)
            : base(pipelineFilter, options)
        {
            _packageEncoder = packageEncoder;
        }

        public virtual async ValueTask SendAsync(TSendPackage package)
        {
            await SendAsync(_packageEncoder, package);
        }

        public new IEasyClient<TPackage, TSendPackage> AsClient()
        {
            return this;
        }
    }
}
