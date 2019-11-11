using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

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
            StartTime = DateTime.Now;
            _channel = channel;
        }

        public string SessionID { get; }

        public DateTime StartTime { get; private set; }

        public bool IsConnected { get; private set; }

        public IServerInfo Server { get; private set; }

       IChannel IAppSession.Channel
       {
           get { return _channel; }
       }

        public object DataContext { get; set; }

        public event EventHandler Connected;

        public event EventHandler Closed;
        
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

        internal void OnSessionClosed(EventArgs e)
        {
            IsConnected = false;
            Closed?.Invoke(this, e);
        }

        internal void OnSessionConnected()
        {
            IsConnected = true;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            lock (_channel)
            {
                return _channel.SendAsync(data);
            }
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