using System;
using System.Buffers;
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
    /// <summary>
    /// Represents an application session that manages connection, state, and events.
    /// </summary>
    public class AppSession : IAppSession, ILogger, ILoggerAccessor
    {
        private IConnection _connection;

        /// <summary>
        /// Gets the connection associated with the session.
        /// </summary>
        protected internal IConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession"/> class.
        /// </summary>
        public AppSession()
        {
        }

        /// <summary>
        /// Initializes the session with the specified server and connection.
        /// </summary>
        /// <param name="server">The server information.</param>
        /// <param name="connection">The connection associated with the session.</param>
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

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }

        /// <summary>
        /// Gets the start time of the session.
        /// </summary>
        public DateTimeOffset StartTime { get; private set; }

        /// <summary>
        /// Gets the current state of the session.
        /// </summary>
        public SessionState State { get; private set; } = SessionState.None;

        /// <summary>
        /// Gets the server information associated with the session.
        /// </summary>
        public IServerInfo Server { get; private set; }

        IConnection IAppSession.Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Gets or sets the data context for the session.
        /// </summary>
        public object DataContext { get; set; }

        /// <summary>
        /// Gets the remote endpoint of the session.
        /// </summary>
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

        /// <summary>
        /// Gets the local endpoint of the session.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return _connection?.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the last active time of the session.
        /// </summary>
        public DateTimeOffset LastActiveTime
        {
            get { return _connection?.LastActiveTime ?? DateTimeOffset.MinValue; }
        }

        /// <summary>
        /// Occurs when the session is connected.
        /// </summary>
        public event AsyncEventHandler Connected;

        /// <summary>
        /// Occurs when the session is closed.
        /// </summary>
        public event AsyncEventHandler<CloseEventArgs> Closed;

        private Dictionary<object, object> _items;

        /// <summary>
        /// Gets or sets session-specific data by key.
        /// </summary>
        /// <param name="name">The key of the data.</param>
        /// <returns>The value associated with the key.</returns>
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

        ValueTask IAppSession.SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
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

        /// <summary>
        /// Closes the session asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public virtual async ValueTask CloseAsync()
        {
            await CloseAsync(CloseReason.LocalClosing);
        }

        /// <summary>
        /// Closes the session asynchronously with the specified reason.
        /// </summary>
        /// <param name="reason">The reason for closing the session.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
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

        /// <summary>
        /// Gets the logger associated with the session.
        /// </summary>
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

        /// <summary>
        /// Gets the logger associated with the session.
        /// </summary>
        public ILogger Logger => this as ILogger;

        #endregion
    }
}