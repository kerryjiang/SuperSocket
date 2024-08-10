using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession, ILogger, ILoggerAccessor
    {
        private IConnection _connection;

        protected internal IConnection Connection
        {
            get { return _connection; }
        }

        public AppSession()
        {
            
        }

        void IAppSession.Initialize(IServerInfo server, IConnection connection)
        {
            if (connection is IConnectionWithSessionIdentifier connectionWithSessionIdentifier)
                SessionID = connectionWithSessionIdentifier.SessionIdentifier;
            else                
                SessionID = Guid.NewGuid().ToString();
            
            Server = server;
            StartTime = DateTimeOffset.Now;
            _connection = connection;
            State = SessionState.Initialized;
        }

        public string SessionID { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public SessionState State { get; private set; } = SessionState.None;

        public IServerInfo Server { get; private set; }

        IConnection IAppSession.Connection
        {
            get { return _connection; }
        }

        public object DataContext { get; set; }

        public EndPoint RemoteEndPoint
        {
            get
            {
                var connection = _connection;

                if (connection == null)
                    return null;

                return connection.ProxyInfo?.SourceEndPoint ?? connection.RemoteEndPoint;
            }
        }

        public EndPoint LocalEndPoint
        {
            get { return _connection?.LocalEndPoint; }
        }

        public DateTimeOffset LastActiveTime
        {
            get { return _connection?.LastActiveTime ?? DateTimeOffset.MinValue; }
        }

        public event AsyncEventHandler Connected;

        public event AsyncEventHandler<CloseEventArgs> Closed;
        
        private Dictionary<object, object> _items;

        public object this[object name]
        {
            get
            {
                var items = _items;

                if (items == null)
                    return null;

                object value;
                
                if (items.TryGetValue(name, out value))
                    return value;

                return null;
            }

            set
            {
                lock (this)
                {
                    var items = _items;

                    if (items == null)
                        items = _items = new Dictionary<object, object>();

                    items[name] = value;
                }
            }
        }

        protected virtual ValueTask OnSessionClosedAsync(CloseEventArgs e)
        {
            return new ValueTask();
        }

        internal async ValueTask FireSessionClosedAsync(CloseEventArgs e)
        {
            State = SessionState.Closed;

            await OnSessionClosedAsync(e);

            var closeEventHandler = Closed;

            if (closeEventHandler == null)
                return;

             await closeEventHandler.Invoke(this, e);
        }


        protected virtual ValueTask OnSessionConnectedAsync()
        {
            return new ValueTask();
        }

        internal async ValueTask FireSessionConnectedAsync()
        {
            State = SessionState.Connected;

            await OnSessionConnectedAsync();            

            var connectedEventHandler = Connected;

            if (connectedEventHandler == null)
                return;

            await connectedEventHandler.Invoke(this, EventArgs.Empty);
        }

        ValueTask IAppSession.SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(data, cancellationToken);
        }

        ValueTask IAppSession.SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(packageEncoder, package, cancellationToken);
        }

        void IAppSession.Reset()
        {
            ClearEvent(ref Connected);
            ClearEvent(ref Closed);
            _items?.Clear();
            State = SessionState.None;
            _connection = null;
            DataContext = null;
            StartTime = default(DateTimeOffset);
            Server = null;

            Reset();
        }

        protected virtual void Reset()
        {

        }

        private void ClearEvent<TEventHandler>(ref TEventHandler sessionEvent)
            where TEventHandler : Delegate
        {
            if (sessionEvent == null)
                return;

            foreach (var handler in sessionEvent.GetInvocationList())
            {
                sessionEvent = Delegate.Remove(sessionEvent, handler) as TEventHandler;
            }
        }

        public virtual async ValueTask CloseAsync()
        {
            await CloseAsync(CloseReason.LocalClosing);
        }

        public virtual async ValueTask CloseAsync(CloseReason reason)
        {
            var connection = Connection;

            if (connection == null)
                return;
            
            try
            {
                await connection.CloseAsync(reason);
            }
            catch
            {
            }
        }

        #region ILogger

        ILogger GetLogger()
        {
            return (Server as ILoggerAccessor).Logger;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetLogger().Log<TState>(logLevel, eventId, state, exception, (s, e) =>
            {
                return $"Session[{this.SessionID}]: {formatter(s, e)}";
            });
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return GetLogger().IsEnabled(logLevel);
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return GetLogger().BeginScope<TState>(state);
        }

        public ILogger Logger => this as ILogger;

        #endregion
    }
}