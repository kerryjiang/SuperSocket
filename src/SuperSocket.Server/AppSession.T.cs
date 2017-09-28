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

            while (true)
            {
                var result = await input.ReadAsync();

                if (result.IsCompleted)
                    break;
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