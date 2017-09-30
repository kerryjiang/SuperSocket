using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class AppSession<TPackageInfo> : AppSession
        where TPackageInfo : class
    {
        IPipelineFilter<TPackageInfo> _pipelineFilter;
        public AppSession(IPipelineFilter<TPackageInfo> pipelineFilter)
        {
            _pipelineFilter = pipelineFilter;
        }

        public override async Task ProcessRequest()
        {
            var input = PipeConnection.Input;

            var consumed = new ReadCursor();
            var examined = new ReadCursor();

            var currentPipelineFilter = _pipelineFilter;

            while (true)
            {
                var result = await input.ReadAsync();

                if (result.IsCompleted)
                {
                    OnClosed();
                    break;
                }

                var buffer = result.Buffer;

                var filterResult = currentPipelineFilter.Filter(buffer, out consumed, out examined);

                switch (filterResult.State)
                {
                    case (ProcessState.Cached):
                        continue;
                    
                    case (ProcessState.Error):
                        break;

                    default:
                        OnPackageReceived(filterResult.Package);
                        break;
                }

                if (currentPipelineFilter.NextFilter != null)
                    _pipelineFilter = currentPipelineFilter.NextFilter;

            }

            await Task.CompletedTask;
        }
        
        private Action<IAppSession, TPackageInfo> _packageReceived;

        public event Action<IAppSession, TPackageInfo> PackageReceived
        {
            add { _packageReceived += value; }
            remove { _packageReceived -= value; }
        }

        protected void OnPackageReceived(TPackageInfo package)
        {
            _packageReceived?.Invoke(this, package);
        }
    }
}