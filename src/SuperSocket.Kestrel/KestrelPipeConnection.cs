namespace SuperSocket.Kestrel;

using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
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

    protected override void OnClosed()
    {
        if (!CloseReason.HasValue)
            CloseReason = Connection.CloseReason.RemoteClosing;

        base.OnClosed();
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

    protected override Task StartInputPipeTask<TPackageInfo>(IObjectPipe<TPackageInfo> packagePipe, CancellationToken cancellationToken)
    {
        cancellationToken.Register(SupplyEnd);
        return base.StartInputPipeTask(packagePipe, cancellationToken);
    }

    protected override async Task OnInputPipeReadAsync(ReadResult result)
    {
        if (result is { IsCanceled: false, IsCompleted: false })
            UpdateLastActiveTime();

        await SupplyRequiredAsync();
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

    protected override void OnReaderComplete(PipeReader reader, bool isDetaching)
    {
        if (isDetaching)
            return;
        
        reader.Complete();
    }

    protected override bool IsIgnorableException(Exception e)
    {
        if (e is IOException ioe && ioe.InnerException != null)
        {
            return IsIgnorableException(ioe.InnerException);
        }

        if (e is SocketException se)
        {
            return se.IsIgnorableSocketException();
        }

        return base.IsIgnorableException(e);
    }

    private void OnConnectionClosed()
    {
        Cancel();
    }
}
