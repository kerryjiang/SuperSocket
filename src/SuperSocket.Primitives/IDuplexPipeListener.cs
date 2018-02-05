using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IDuplexPipeListener
    {
        void Start(IPEndPoint endpoint, Func<IDuplexPipe, Task> callback);

        void Stop();
    }
}