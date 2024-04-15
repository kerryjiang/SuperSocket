using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server.Abstractions.Session
{
    public interface IAppSession
    {
        string SessionID { get; }

        DateTimeOffset StartTime { get; }

        DateTimeOffset LastActiveTime { get; }

        IConnection Connection { get; }

        EndPoint RemoteEndPoint { get; }

        EndPoint LocalEndPoint { get; }

        ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default);

        ValueTask CloseAsync(CloseReason reason);

        IServerInfo Server { get; }

        event AsyncEventHandler Connected;

        event AsyncEventHandler<CloseEventArgs> Closed;

        object DataContext { get; set; }

        void Initialize(IServerInfo server, IConnection connection);

        object this[object name] { get; set; }

        SessionState State { get; }

        void Reset();
    }
}