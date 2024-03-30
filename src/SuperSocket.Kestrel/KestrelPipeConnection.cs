namespace SuperSocket.Kestrel;

using System;
using System.IO.Pipelines;
using System.Threading;
using Microsoft.AspNetCore.Connections;
using SuperSocket.Connection;

public class KestrelPipeConnection : PipeConnectionBase
{
    private ConnectionContext _context;

    public KestrelPipeConnection(ConnectionContext context, ConnectionOptions options)
        : base(context.Transport.Input, context.Transport.Output, options)
    {
        _context = context;
        context.ConnectionClosed.Register(() => OnClosed());
        LocalEndPoint = context.LocalEndPoint;
        RemoteEndPoint = context.RemoteEndPoint;
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
    }
}
