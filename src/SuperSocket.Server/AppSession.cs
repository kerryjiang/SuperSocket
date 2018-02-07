using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;

namespace SuperSocket.Server
{
    public abstract class AppSession : IAppSession
    {
        private IDuplexPipe _pipe;

        protected IDuplexPipe Pipe
        {
            get { return _pipe; }
        }

        public void Initialize(IDuplexPipe pipe)
        {
            _pipe = pipe;
        }

        public abstract Task ProcessRequest();

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

        public Task SendAsync(ReadOnlySpan<byte> buffer)
        {
            var pipe = _pipe;
            pipe.Output.Write(buffer);
            return FlushAsync(pipe.Output);
        }

        async Task FlushAsync(PipeWriter buffer)
        {
            await buffer.FlushAsync();
        }
    }
}