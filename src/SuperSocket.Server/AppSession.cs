using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession
    {
        private IChannel _channel;

        protected internal IChannel Channel
        {
            get { return _channel; }
        }

        public AppSession()
        {
            SessionID = Guid.NewGuid().ToString();
        }

        void IAppSession.Initialize(IServerInfo server, IChannel channel)
        {
            Server = server;
            StartTime = DateTimeOffset.Now;
            _channel = channel;
        }

        public string SessionID { get; }

        public DateTimeOffset StartTime { get; private set; }

        public SessionState State { get; private set; } = SessionState.Initialized;

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

        public event AsyncEventHandler Closed;
        
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

        protected virtual ValueTask OnSessionClosedAsync(EventArgs e)
        {
            return new ValueTask();
        }

        internal async ValueTask FireSessionClosedAsync(EventArgs e)
        {
            State = SessionState.Closed;

            await OnSessionClosedAsync(e);

            var closeEventHandler = Closed;

            if (closeEventHandler == null)
                return;

            await closeEventHandler.Invoke(this, EventArgs.Empty);
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

        public void Close()
        {
            try
            {
                Channel?.Close();
            }
            catch
            {
            }
        }
    }
}