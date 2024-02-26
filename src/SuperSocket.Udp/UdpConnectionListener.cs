using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Udp
{
    class UdpConnectionListener : IConnectionListener
    {
        private ILogger _logger;

        private Socket _listenSocket;

        private IPEndPoint _acceptRemoteEndPoint;

        public IConnectionFactory ConnectionFactory { get; }
        
        public ListenOptions Options { get;  }

        public ConnectionOptions ConnectionOptions { get; }

        public bool IsRunning { get; private set; }

        public event NewConnectionAcceptHandler NewConnectionAccept;

        private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        private CancellationTokenSource _cancellationTokenSource;

        private TaskCompletionSource<bool> _stopTaskCompletionSource;

        private IUdpSessionIdentifierProvider _udpSessionIdentifierProvider;

        private IAsyncSessionContainer _sessionContainer;

        public UdpConnectionListener(ListenOptions options, ConnectionOptions connectionOptions, IConnectionFactory connectionFactory, ILogger logger, IUdpSessionIdentifierProvider udpSessionIdentifierProvider, IAsyncSessionContainer sessionContainer)
        {
            Options = options;
            ConnectionOptions = connectionOptions;
            ConnectionFactory = connectionFactory;
            _logger = logger;
            _udpSessionIdentifierProvider = udpSessionIdentifierProvider;
            _sessionContainer = sessionContainer;
        }

        public bool Start()
        {
            var options = Options;

            try
            {                
                var listenEndpoint = options.ToEndPoint();                
                var listenSocket = _listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                
                if (options.NoDelay)
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                
                listenSocket.Bind(listenEndpoint);

                _acceptRemoteEndPoint = listenEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);

                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];

                try
                {
                    listenSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                }
                catch (PlatformNotSupportedException)
                {
                    _logger.LogWarning("Failed to set socket option SIO_UDP_CONNRESET because the platform doesn't support it.");
                }                

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAccept(listenSocket).DoNotAwait();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
                return false;
            }
        }

        private async Task KeepAccept(Socket listenSocket)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var buffer = default(byte[]);

                try
                {
                    var bufferSize = ConnectionOptions.MaxPackageLength;
                    buffer = _bufferPool.Rent(bufferSize);

                    var result = await listenSocket
                        .ReceiveFromAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, _acceptRemoteEndPoint)
                        .ConfigureAwait(false);

                    var packageData = new ArraySegment<byte>(buffer, 0, result.ReceivedBytes);
                    var remoteEndPoint = result.RemoteEndPoint as IPEndPoint;
                    
                    var sessionID = _udpSessionIdentifierProvider.GetSessionIdentifier(remoteEndPoint, packageData);

                    var session = await _sessionContainer.GetSessionByIDAsync(sessionID);

                    IVirtualConnection connection  = null;

                    if (session != null)
                    {
                        connection = session.Connection as IVirtualConnection;
                    }
                    else
                    {
                        connection = await CreateConnection(_listenSocket, remoteEndPoint, sessionID) as IVirtualConnection;

                        if (connection == null)
                            return;

                        OnNewConnectionAccept(connection);
                    }

                    await connection.WritePipeDataAsync(packageData.AsMemory(), _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException || e is NullReferenceException)
                        break;
                    
                    if (e is SocketException se)
                    {
                        var errorCode = se.ErrorCode;

                        //The listen socket was closed
                        if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                        {
                            break;
                        }
                    }
                    
                    _logger.LogError(e, $"Listener[{this.ToString()}] failed to receive udp data");
                }
                finally
                {
                    _bufferPool.Return(buffer);
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        private void OnNewConnectionAccept(IConnection connection)
        {
            var handler = NewConnectionAccept;

            if (handler == null)
                return;

            handler.Invoke(Options, connection);
        }

        private async ValueTask<IConnection> CreateConnection(Socket socket, IPEndPoint remoteEndPoint, string sessionIdentifier)
        {
            try
            {
                return await ConnectionFactory.CreateConnection(new UdpConnectionInfo
                {
                    Socket = socket,
                    SessionIdentifier = sessionIdentifier,
                    RemoteEndPoint = remoteEndPoint
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create channel for {socket.RemoteEndPoint}.");
                return null;
            }   
        }

        public async Task<IConnection> CreateConnection(object connection)
        {
            var socket = (Socket)connection;
            var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            return await CreateConnection(socket, remoteEndPoint, _udpSessionIdentifierProvider.GetSessionIdentifier(remoteEndPoint, null));
        }

        public Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return Task.Delay(0);

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            listenSocket.Close();
            
            return _stopTaskCompletionSource.Task;
        }

        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}