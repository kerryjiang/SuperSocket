using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    class HandshakeCheckMiddleware : MiddlewareBase
    {
        private ConcurrentQueue<WebSocketSession> _openHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();

        private ConcurrentQueue<WebSocketSession> _closeHandshakePendingQueue = new ConcurrentQueue<WebSocketSession>();
        
        private const int _handshakePendingQueueCheckingInterval = 60;// 1 minute by default

        private const int _openHandshakeTimeOut = 120;// 2 minutes default

        private const int _closeHandshakeTimeOut = 120;// 2 minutes default

        private Timer _checkingTimer;

        public override void Register(IServer server)
        {
            _checkingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, _handshakePendingQueueCheckingInterval * 1000, _handshakePendingQueueCheckingInterval * 1000); // hardcode to 1 minute for now
        }


        public override ValueTask<bool> HandleSession(IAppSession session)
        {
            _openHandshakePendingQueue.Enqueue(session as WebSocketSession);
            (session as WebSocketSession).CloseHandshakeStarted += OnCloseHandshakeStarted;            
            return new ValueTask<bool>(true);
        }

        private void OnCloseHandshakeStarted(object sender, EventArgs e)
        {
            var session = sender as WebSocketSession;
            session.CloseHandshakeStarted -= OnCloseHandshakeStarted;
            _closeHandshakePendingQueue.Enqueue(session);
        }

        internal void EnqueueClosingSession(WebSocketSession session)
        {
            _closeHandshakePendingQueue.Enqueue(session);
        }

        private void HandshakePendingQueueCheckingCallback(object state)
        {
            _checkingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            while (true)
            {
                WebSocketSession session;

                if (!_openHandshakePendingQueue.TryPeek(out session))
                    break;

                if (session.Handshaked || !session.IsConnected)
                {
                    //Handshaked or not connected
                    _openHandshakePendingQueue.TryDequeue(out session);
                    continue;
                }

                if (DateTime.Now < session.StartTime.AddSeconds(_openHandshakeTimeOut))
                    break;

                //Timeout, dequeue and then close
                _openHandshakePendingQueue.TryDequeue(out session);
                session.Close();
            }

            while (true)
            {
                WebSocketSession session;

                if (!_closeHandshakePendingQueue.TryPeek(out session))
                    break;

                if (!session.IsConnected)
                {
                    //the session has been closed
                    _closeHandshakePendingQueue.TryDequeue(out session);
                    continue;
                }

                if (DateTime.Now < session.CloseHandshakeStartTime.AddSeconds(_closeHandshakeTimeOut))
                    break;

                //Timeout, dequeue and then close
                _closeHandshakePendingQueue.TryDequeue(out session);
                //Needn't send closing handshake again
                session.Close();
            }

            _checkingTimer.Change(_handshakePendingQueueCheckingInterval * 1000, _handshakePendingQueueCheckingInterval * 1000);
        }        
    }
}