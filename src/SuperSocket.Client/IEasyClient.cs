using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    public interface IEasyClient
    {
        IConnector Proxy { get; set; }

        ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken = default);

        IPEndPoint LocalEndPoint { get; set; }

        SecurityOptions Security { get; set; }

        void StartReceive();

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package);

        event EventHandler Closed;

        ValueTask CloseAsync();
    }


    public interface IEasyClient<TReceivePackage> : IEasyClient
        where TReceivePackage : class
    {
        ValueTask<TReceivePackage> ReceiveAsync();

        event PackageHandler<TReceivePackage> PackageHandler;
    }

    public interface IEasyClient<TReceivePackage, TSendPackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        ValueTask SendAsync(TSendPackage package);
    }
}