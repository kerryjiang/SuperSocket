using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public interface IAppSession
    {
        void Initialize(IPipeConnection pipeConnection);

        Task ProcessRequest();

        event EventHandler Closed;
    }
}