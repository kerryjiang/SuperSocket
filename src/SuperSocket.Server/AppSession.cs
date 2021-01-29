using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession, ILogger, ILoggerAccessor
    {
        private IChannel _channel;

        protected internal IChannel Channel
        {
            get { return _channel; }
        }

        public AppSession()
        {
            
        }

        void IAppSession.Initialize(IServerInfo server, IChannel channel)
        {
            if (channel is IChannelWithSessionIdentifier channelWithSessionIdentifier)
                SessionID = channelWithSessionIdentifier.SessionIdentifier;
            else                
                SessionID = Guid.NewGuid().ToString();
            
            Server = server;
            StartTime = DateTimeOffset.Now;
            _channel = channel;
            State = SessionState.Initialized;
        }

        public string SessionID { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public SessionState State { get; private set; } = SessionState.None;

        public IServerInfo Server { get; private set; }

        IChannel IAppSession.Channel
        {
            get { return _channel; }
        }

        public object DataContext { get; set; }

        public EndPoint RemoteEndPoint
        {
            get { return _channel?.RemoteEndPoint; }
        }

        public EndPoint LocalEndPoint
        {
            get { return _channel?.LocalEndPoint; }
        }

        public DateTimeOffset LastActiveTime
        {
            get { return _channel?.LastActiveTime ?? DateTimeOffset.MinValue; }
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

        ValueTask IAppSession.SendAsync(ReadOnlyMemory<byte> data)
        {
            return _channel.SendAsync(data);
        }

        ValueTask IAppSession.SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package)
        {
            return _channel.SendAsync(packageEncoder, package);
        }

        void IAppSession.Reset()
        {
            ClearEvent(ref Connected);
            ClearEvent(ref Closed);
            _items?.Clear();
            State = SessionState.None;
            _channel = null;
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
            var channel = Channel;

            if (channel == null)
                return;
            
            try
            {
                await channel.CloseAsync(reason);
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