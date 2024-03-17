using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;

namespace SuperSocket.Server.Connection
{
    public interface IConnectionStreamInitializersFactory
    {
        IEnumerable<IConnectionStreamInitializer> Create(ListenOptions listenOptions);
    }
}