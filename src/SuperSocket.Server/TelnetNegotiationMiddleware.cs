using System;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class TelnetNegotiationMiddleware : MiddlewareBase
    {
        private static readonly byte[] _handshakeData = new byte[]
            {
                0xff, 0xfd, 0x24,   // IAC DO LINEMODE
                0xff, 0xfd, 0x01,   // Do Echo
                0xff, 0xfd, 0x21,   // Do Remote Flow Control
                0xff, 0xfb, 0x01,   // Will Echo
                0xff, 0xfb, 0x03    // Will Supress Go Ahead
            };
        
        public override void Register(IServer server, IAppSession session)
        {
            session.Connected += OnSessionConnected;
        }

        private void OnSessionConnected(object sender, EventArgs e)
        {
            var session = sender as IAppSession;            
            session.Connected -= OnSessionConnected;            
            SendHandshakeAsync(session.Channel);
        }

        private async void SendHandshakeAsync(IChannel channel)
        {
            await channel.SendAsync(_handshakeData);
        }
    }
}
