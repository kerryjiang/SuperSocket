using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.WebSocket.Server
{
    internal interface IWebSocketServerMiddleware
    {
        ValueTask HandleSessionHandshakeCompleted(WebSocketSession session);

        int OpenHandshakePendingQueueLength { get; }

        int CloseHandshakePendingQueueLength { get; }
    }

    /// <summary>
    /// Represents middleware for managing WebSocket server sessions.
    /// </summary>
    class WebSocketServerMiddleware : MiddlewareBase, IWebSocketServerMiddleware
    {
        private ConcurrentQueue<WebSocketSession> _openHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();

        private ConcurrentQueue<WebSocketSession> _closeHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();
        
        private Timer _checkingTimer;

        private readonly HandshakeOptions _options;

        private IMiddleware _sessionContainerMiddleware;

        private ISessionEventHost _sessionEventHost;

        public int OpenHandshakePendingQueueLength
        {
            get { return _openHandshakePendingQueue.Count;  }
        }

        public int CloseHandshakePendingQueueLength
        {
            get { return _closeHandshakePendingQueue.Count;  }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServerMiddleware"/> class.
        /// </summary>
        /// <param name="handshakeOptions">The handshake options.</param>
        public WebSocketServerMiddleware(IOptions<HandshakeOptions> handshakeOptions)
        {
            var options = handshakeOptions.Value;

            if (options == null)
                options = new HandshakeOptions();

            _options = options;        
        }

        /// <summary>
        /// Starts the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public override void Start(IServer server)
        {
            _sessionContainerMiddleware = server.GetSessionContainer() as IMiddleware;
            _sessionEventHost = server as ISessionEventHost;
            _checkingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, _options.CheckingInterval * 1000, _options.CheckingInterval * 1000); // hardcode to 1 minute for now
        }

        /// <summary>
        /// Shuts down the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public override void Shutdown(IServer server)
        {
            _sessionContainerMiddleware = null;

            var checkTimer = _checkingTimer;

            if (checkTimer == null)
                return;

            if (Interlocked.CompareExchange(ref _checkingTimer, null, checkTimer) == checkTimer)
            {
                checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                checkTimer.Dispose();
            }            
        }

        /// <summary>
        /// Registers a session with the middleware asynchronously.
        /// </summary>
        /// <param name="session">The session to register.</param>
        /// <returns>A task that represents the asynchronous registration operation.</returns>
        public override ValueTask<bool> RegisterSession(IAppSession session)
        {
            var websocketSession = session as WebSocketSession;
            _openHandshakePendingQueue.Enqueue(websocketSession);
            return new ValueTask<bool>(true);
        }

        private void OnCloseHandshakeStarted(object sender, EventArgs e)
        {
            var session = sender as WebSocketSession;
            session.CloseHandshakeStarted -= OnCloseHandshakeStarted;
            _closeHandshakePendingQueue.Enqueue(session);
        }

        private void HandshakePendingQueueCheckingCallback(object state)
        {
            _checkingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            var openHandshakeTimeTask = Task.Run(() =>
            {
                while (true)
                {
                    WebSocketSession session;

                    if (!_openHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (session.Handshaked || session.State == SessionState.Closed || (session is IAppSession appSession && appSession.Connection.IsClosed))
                    {
                        //Handshaked or not connected
                        _openHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartTime.AddSeconds(_options.OpenHandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    _openHandshakePendingQueue.TryDequeue(out session);
                    session.CloseWithoutHandshake();
                }
            });

            var closeHandshakeTimeTask = Task.Run(() =>
            {
                while (true)
                {
                    WebSocketSession session;

                    if (!_closeHandshakePendingQueue.TryPeek(out session))
                        break;

                    if (session.State == SessionState.Closed)
                    {
                        //the session has been closed
                        _closeHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.CloseHandshakeStartTime.AddSeconds(_options.CloseHandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    _closeHandshakePendingQueue.TryDequeue(out session);
                    //Needn't send closing handshake again
                    session.CloseWithoutHandshake();
                }
            });

            Task.WhenAll(openHandshakeTimeTask, closeHandshakeTimeTask);

            _checkingTimer?.Change(_options.CheckingInterval * 1000, _options.CheckingInterval * 1000);
        }

        /// <summary>
        /// Handles the completion of a session handshake.
        /// </summary>
        /// <param name="session">The WebSocket session.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public ValueTask HandleSessionHandshakeCompleted(WebSocketSession session)
        {
            session.CloseHandshakeStarted += OnCloseHandshakeStarted;
            _sessionContainerMiddleware?.RegisterSession(session);
            return _sessionEventHost.HandleSessionConnectedEvent(session);
        }
    }
}