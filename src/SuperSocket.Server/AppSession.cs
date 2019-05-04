using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession
    {
        public AppSession()
        {
            SessionID = Guid.NewGuid().ToString();
        }

        void IAppSession.Initialize(IServerInfo server, IChannel channel)
        {
            Server = server;
            Channel = channel;
            channel.Closed += OnSessionClosed;
        }

        public string SessionID { get; }

        public IServerInfo Server { get; private set; }

        public IChannel Channel { get; private set; }

        public object State { get; set; }

        public event EventHandler Connected;

        public event EventHandler Closed;

        private void OnSessionClosed(object sender, EventArgs e)
        {
            var channel = sender as IChannel;
            channel.Closed -= OnSessionClosed;
            Closed?.Invoke(this, e);
        }

        internal void OnSessionConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
    }
}