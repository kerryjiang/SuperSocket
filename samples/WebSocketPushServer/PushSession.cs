using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server;
using SuperSocket.SessionContainer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace WebSocketPushServer
{
    public class PushSession : WebSocketSession
    {
        private int _messageSent;

        public int MessageSent
        {
            get { return _messageSent; }
        }

        private int _messageClientReceived;

        public int MessageClientReceived
        {
            get { return _messageClientReceived; }
        }

        protected override async ValueTask OnSessionConnectedAsync()
        {
            await this.SendAsync(this.SessionID);
        }

        public void Ack()
        {
            Interlocked.Increment(ref _messageClientReceived);
        }

        public override async ValueTask SendAsync(string message)
        {
            await base.SendAsync(message);
            Interlocked.Increment(ref _messageSent);
        }

        public void PrintStats()
        {
            var speed = (double)_messageSent / (double)this.LastActiveTime.Subtract(this.StartTime).TotalSeconds;
            Console.WriteLine($"Sent {_messageSent} messages, received {_messageClientReceived} messages, {speed:F1}.");
        }
    }
}
