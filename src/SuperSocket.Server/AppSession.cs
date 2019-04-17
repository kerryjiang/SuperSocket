using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class AppSession : IAppSession
    {
        internal AppSession(IServerInfo server, IChannel channel)
        {
            Server = server;
            Channel = channel;
            SessionID = Guid.NewGuid().ToString();
            channel.Closed += OnSessionClosed;
        }

        public string SessionID { get; }

        public IServerInfo Server { get; }

        public IChannel Channel { get; }

        public EventHandler Closed;

        private void OnSessionClosed(object sender, EventArgs e)
        {
            var channel = sender as IChannel;
            channel.Closed -= OnSessionClosed;
            Closed?.Invoke(this, e);
        }
    }
}