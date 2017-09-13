using System;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public abstract class ChannelBase : IChannel
    {
        public abstract Task<ArraySegment<byte>> ReceiveAsync();
        public abstract Task SendAsync(ArraySegment<byte> data);
        public abstract void Close();

        private EventHandler _closed;

        public event EventHandler Closed
        {
            add { _closed += value; }
            remove { _closed -= value; }
        }

        protected virtual void OnClosed()
        {
            _closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
