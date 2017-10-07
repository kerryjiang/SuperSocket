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

            var currentPipelineFilter = _pipelineFilter;

            while (true)
            {
                var result = await input.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    if (result.IsCompleted)
                    {
                        OnClosed();
                        break;
                    }

                    while (true)
                    {
                        var packageInfo = currentPipelineFilter.Filter(ref buffer);

                        if (currentPipelineFilter.NextFilter != null)
                            _pipelineFilter = currentPipelineFilter = currentPipelineFilter.NextFilter;
                    
                        // continue receive...
                        if (packageInfo == null)
                            break;

                        // already get a package
                        OnPackageReceived(packageInfo);

                        if (buffer.Length == 0) // no more data
                            break;
                    }
                }
                finally
                {
                    input.Advance(buffer.Start, buffer.End);
                }
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