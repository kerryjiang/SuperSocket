
using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    public delegate ValueTask NewConnectionAcceptHandler(ListenOptions listenOptions, IConnection connection);

    public interface IConnectionListener
    {
        ListenOptions Options { get; }

        bool Start();

        event NewConnectionAcceptHandler NewConnectionAccept;

        Task StopAsync();

        bool IsRunning { get; }

        IConnectionFactory ConnectionFactory { get; }
    }
}