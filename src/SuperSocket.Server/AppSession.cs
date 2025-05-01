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

        /// <summary>
        /// Called when the session is closed.
        /// </summary>
        /// <param name="e">The close event arguments containing the reason for closing.</param>
        /// <returns>A task representing the async operation.</returns>
        protected virtual ValueTask OnSessionClosedAsync(CloseEventArgs e)
        {
            return new ValueTask();
        }

        /// <summary>
        /// Fires the session closed event.
        /// </summary>
        /// <param name="e">The close event arguments containing the reason for closing.</param>
        /// <returns>A task representing the async operation.</returns>
        internal async ValueTask FireSessionClosedAsync(CloseEventArgs e)
        {
            State = SessionState.Closed;

            await OnSessionClosedAsync(e);

            var closeEventHandler = Closed;

            if (closeEventHandler == null)
                return;

            await closeEventHandler.Invoke(this, e);
        }

        /// <summary>
        /// Called when the session is connected.
        /// </summary>
        /// <returns>A task representing the async operation.</returns>
        protected virtual ValueTask OnSessionConnectedAsync()
        {
            return new ValueTask();
        }

        /// <summary>
        /// Fires the session connected event.
        /// </summary>
        /// <returns>A task representing the async operation.</returns>
        internal async ValueTask FireSessionConnectedAsync()
        {
            State = SessionState.Connected;

            await OnSessionConnectedAsync();

            var connectedEventHandler = Connected;

            if (connectedEventHandler == null)
                return;

            await connectedEventHandler.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sends binary data to the client asynchronously.
        /// </summary>
        /// <param name="data">The binary data to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the async send operation.</returns>
        ValueTask IAppSession.SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(data, cancellationToken);
        }

        /// <summary>
        /// Sends readonly sequence data to the client asynchronously.
        /// </summary>
        /// <param name="data">The binary data to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the async send operation.</returns>
        ValueTask IAppSession.SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(data, cancellationToken);
        }

        /// <summary>
        /// Sends a package to the client asynchronously.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package.</typeparam>
        /// <param name="packageEncoder">The encoder used to encode the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the async send operation.</returns>
        ValueTask IAppSession.SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken)
        {
            return _connection.SendAsync(packageEncoder, package, cancellationToken);
        }

        /// <summary>
        /// Resets the session state. Called by the server when recycling the session.
        /// </summary>
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

        /// <summary>
        /// Called when the session is reset. Derived classes can override this method to perform additional cleanup.
        /// </summary>
        protected virtual void Reset()
        {
        }

        /// <summary>
        /// Clears all handlers from an event.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="sessionEvent">The event to clear.</param>
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
        /// Gets the logger associated with the session from the server.
        /// </summary>
        /// <returns>The logger instance.</returns>
        ILogger GetLogger()
        {
            return (Server as ILoggerAccessor).Logger;
        }

        /// <summary>
        /// Writes a log entry with the specified log level, event ID, state, exception, and formatter.
        /// Prefixes log entries with the session ID.
        /// </summary>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        /// <param name="logLevel">The level of the log entry.</param>
        /// <param name="eventId">The event ID for the log entry.</param>
        /// <param name="state">The state to be logged.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">The function to format the state and exception into a log message.</param>
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetLogger().Log<TState>(logLevel, eventId, state, exception, (s, e) =>
            {
                return $"Session[{this.SessionID}]: {formatter(s, e)}";
            });
        }

        /// <summary>
        /// Checks if the given log level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level to check.</param>
        /// <returns>True if the log level is enabled; otherwise, false.</returns>
        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return GetLogger().IsEnabled(logLevel);
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
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