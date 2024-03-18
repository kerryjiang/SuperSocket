using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionStreamInitializersFactory
    {
        IEnumerable<IConnectionStreamInitializer> Create(ListenOptions listenOptions);
    }
}