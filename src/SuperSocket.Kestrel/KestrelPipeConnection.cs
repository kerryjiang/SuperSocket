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

/// <summary>
/// Represents a pipe connection that integrates with Kestrel's <see cref="ConnectionContext"/>.
/// </summary>
public class KestrelPipeConnection : PipeConnectionBase
{
    private ConnectionContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="KestrelPipeConnection"/> class with the specified connection context and options.
    /// </summary>
    /// <param name="context">The Kestrel connection context.</param>
    /// <param name="options">The connection options.</param>
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

    /// <summary>
    /// Completes the reader asynchronously.
    /// </summary>
    /// <param name="reader">The pipe reader to complete.</param>
    /// <param name="isDetaching">Indicates whether the connection is detaching.</param>
    protected override async ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
    {
        if (!isDetaching)
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Completes the writer asynchronously.
    /// </summary>
    /// <param name="writer">The pipe writer to complete.</param>
    /// <param name="isDetaching">Indicates whether the connection is detaching.</param>
    protected override async ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
    {
        if (!isDetaching)
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles the closure of the connection.
    /// </summary>
    protected override void OnClosed()
    {
        if (!CloseReason.HasValue)
            CloseReason = Connection.CloseReason.RemoteClosing;

        base.OnClosed();
    }

    /// <summary>
    /// Closes the connection and disposes the context.
    /// </summary>
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

    /// <summary>
    /// Updates the last active time when input pipe data is read.
    /// </summary>
    /// <param name="result">The result of the pipe read operation.</param>
    protected override void OnInputPipeRead(ReadResult result)
    {
        if (result is { IsCanceled: false, IsCompleted: false })
            UpdateLastActiveTime();
    }

    /// <summary>
    /// Sends data over the connection asynchronously using a custom write action.
    /// </summary>
    /// <param name="write">The action to write data to the pipe writer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken)
    {
        await base.SendAsync(write, cancellationToken);
        UpdateLastActiveTime();
    }

    /// <summary>
    /// Sends data over the connection asynchronously.
    /// </summary>
    /// <param name="buffer">The data to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        await base.SendAsync(buffer, cancellationToken);
        UpdateLastActiveTime();
    }

    /// <summary>
    /// Sends a package over the connection asynchronously using the specified encoder.
    /// </summary>
    /// <typeparam name="TPackage">The type of the package to send.</typeparam>
    /// <param name="packageEncoder">The encoder to use for the package.</param>
    /// <param name="package">The package to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package,
        CancellationToken cancellationToken)
    {
        await base.SendAsync(packageEncoder, package, cancellationToken);
        UpdateLastActiveTime();
    }

    /// <summary>
    /// Determines whether the specified exception is ignorable.
    /// </summary>
    /// <param name="e">The exception to check.</param>
    /// <returns><c>true</c> if the exception is ignorable; otherwise, <c>false</c>.</returns>
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