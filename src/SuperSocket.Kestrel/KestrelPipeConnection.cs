using Microsoft.Extensions.Logging;

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

        if (options.ReadAsDemand)
        {
            Logger.LogWarning($"{nameof(KestrelPipeConnection)} doesn't support ReadAsDemand.");
        }
    }

    protected override async ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
    {
        if (!isDetaching)
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }

    protected override async ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
    {
        if (!isDetaching)
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }
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

    protected override void OnInputPipeRead(ReadResult result)
    {
        if (result is { IsCanceled: false, IsCompleted: false })
            UpdateLastActiveTime();
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

    public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package,
        CancellationToken cancellationToken)
    {
        await base.SendAsync(packageEncoder, package, cancellationToken);
        UpdateLastActiveTime();
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
        CancelAsync().Wait();
    }
}