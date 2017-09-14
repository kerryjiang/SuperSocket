using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IPipelineConnectionListener
    {
        void Start(IPEndPoint endpoint);

        void Stop();

        void OnConnection(Func<IPipeConnection, Task> callback);
    }
}