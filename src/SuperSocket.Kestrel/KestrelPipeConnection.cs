namespace SuperSocket.Kestrel;

using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

public class KestrelPipeConnection : PipeConnectionBase
{
    private ConnectionContext _context;

    public KestrelPipeConnection(ConnectionContext context, ConnectionOptions options)
        : base(context.Transport.Input, context.Transport.Output, options)
    {
        _context = context;
        context.ConnectionClosed.Register(() => OnConnectionClosed());
        LocalEndPoint = context.LocalEndPoint;
        RemoteEndPoint = context.RemoteEndPoint;
    }

    public override ValueTask DetachAsync()
    {
        throw new NotSupportedException($"Detach is not supported by {nameof(KestrelPipeConnection)}.");
    }

    protected override async void Close()
    {
        var context = _context;

        if (context == null)
            return;

        if (Interlocked.CompareExchange(ref _context, null, context) == context)
        {
            await context.DisposeAsync();
        }
    }

    protected override void OnClosed()
    {
        if (!CloseReason.HasValue)
            CloseReason = Connection.CloseReason.RemoteClosing;

        base.OnClosed();
        Cancel();
    }

    protected override void OnInputPipeRead(ReadResult result)
    {
        if (!result.IsCanceled && !result.IsCompleted)
        {
            UpdateLastActiveTime();
        }
    }

    public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken)
    {
        await base.SendAsync(write, cancellationToken);
        UpdateLastActiveTime();
    }

    public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        await base.SendAsync(buffer, cancellationToken);
        UpdateLastActiveTime();
    }

    public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken)
    {
        await base.SendAsync(packageEncoder, package, cancellationToken);
        UpdateLastActiveTime();
    }

    private void OnConnectionClosed()
    {
        OnClosed();
        Cancel();
    }
}
