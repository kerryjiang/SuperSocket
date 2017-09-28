using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public abstract class AppSession : IAppSession
    {
        private IPipeConnection _pipeConnection;

        protected IPipeConnection PipeConnection
        {
            get { return _pipeConnection; }
        }

        public void Initialize(IPipeConnection pipeConnection)
        {
            _pipeConnection = pipeConnection;
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
    }
}