using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public class AppSession
    {
        private IPipeConnection _pipeConnection;

        public AppSession(IPipeConnection pipeConnection)
        {
            _pipeConnection = pipeConnection;
        }

        public async Task ProcessRequest()
        {
            var input = _pipeConnection.Input;

            while (true)
            {
                var result = await input.ReadAsync();

                if (result.IsCompleted)
                    break;
            }

            await Task.CompletedTask;
        }

        private EventHandler _closed;

        public event EventHandler Closed
        {
            add { _closed += value; }
            remove { _closed -= value; }
        }

        protected void OnClosed()
        {
            _closed?.Invoke(this, EventArgs.Empty);
        }
    }
}