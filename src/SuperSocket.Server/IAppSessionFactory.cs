using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Server
{
    public interface IAppSessionFactory
    {
        IAppSession Create(IDuplexPipe pipe);
    }
}