using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Kestrel;
using SuperSocket.ProtoBase;

namespace KestrelHttp2DetachDemo;

internal static class Program
{
    private const int Port = 4052;
    private const int TcpPort = 4053;
    private static readonly byte[] ClientPreface = Encoding.ASCII.GetBytes("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n");
    private static int _activeServerConnections;
    private static TimeSpan _serverDetachDelay = TimeSpan.Zero;
    private static TaskCompletionSource _serverHandshakeDetached = CreateSignal();

    internal static int ActiveServerConnections => Volatile.Read(ref _activeServerConnections);

    private static async Task Main(string[] args)
    {
        Console.WriteLine("SuperSocket.Kestrel HTTP/2 detach demo");
        Console.WriteLine("Handshake uses SuperSocket.Kestrel; normal data is sent after DetachAsync.");

        var app = BuildApp(args);
        await using var host = app;

        await app.StartAsync();

        Console.WriteLine($"ASP.NET Core Kestrel server started on 127.0.0.1:{Port}.");

        try
        {
            if (args.Contains("--no-signal-wait", StringComparer.OrdinalIgnoreCase))
            {
                await RunNoSignalWaitDetachDemoAsync(app.Services);
            }
            else
            {
                await RunCleanDetachDemoAsync(app.Services);

                if (args.Contains("--stress-early-raw", StringComparer.OrdinalIgnoreCase))
                {
                    await RunEarlyRawDataDuringDetachDemoAsync(app.Services);
                }
                else if (args.Contains("--compare-tcp", StringComparer.OrdinalIgnoreCase))
                {
                    await RunTcpEarlyRawComparisonDemoAsync(app.Services);
                }
                else
                {
                    Console.WriteLine("Run with --no-signal-wait to verify Kestrel raw handoff without waiting for the in-process server detach signal.");
                    Console.WriteLine("Run with --stress-early-raw to reproduce the unsafe case where raw data arrives before the server wrapper starts detaching.");
                    Console.WriteLine("Run with --compare-tcp to compare the same no-wait handoff over ordinary TcpPipeConnection.");
                    Console.WriteLine("Demo complete.");
                }
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static WebApplication BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddConnections();
        builder.Services.AddSocketConnectionFactory();
        builder.Services.AddSingleton<Http2ConnectionHandler>();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(Port, listenOptions =>
            {
                listenOptions.UseConnectionHandler<Http2ConnectionHandler>();
            });
        });

        return builder.Build();
    }

    private static async Task RunCleanDetachDemoAsync(IServiceProvider services)
    {
        _serverDetachDelay = TimeSpan.Zero;
        ResetServerHandshakeDetachedSignal();

        var connectionFactory = services.GetRequiredService<Microsoft.AspNetCore.Connections.IConnectionFactory>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        await using var context = await connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Loopback, Port));
        Console.WriteLine("Clean scenario: client connected with Kestrel IConnectionFactory.");

        var handshakeConnection = CreateKestrelConnection(context, loggerFactory, "client-handshake-wrapper");
        await RunClientHandshakeAndDetachAsync(context, handshakeConnection);

        Console.WriteLine($"After DetachAsync: wrapper IsClosed={handshakeConnection.IsClosed}, CloseReason={handshakeConnection.CloseReason?.ToString() ?? "<null>"}");
        Console.WriteLine($"After DetachAsync: Kestrel context closed={context.ConnectionClosed.IsCancellationRequested}, active server connections={ActiveServerConnections}");
        await WaitForServerHandshakeDetachedAsync();
        Console.WriteLine("Server-side SuperSocket wrapper has detached; starting raw Kestrel data transfer.");

        await VerifyRawDataAfterDetachAsync(context);

        Console.WriteLine("Raw data after DetachAsync matched exactly; no byte shift or corruption was observed.");
    }

    private static async Task RunNoSignalWaitDetachDemoAsync(IServiceProvider services)
    {
        _serverDetachDelay = TimeSpan.Zero;
        ResetServerHandshakeDetachedSignal();

        var connectionFactory = services.GetRequiredService<Microsoft.AspNetCore.Connections.IConnectionFactory>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        await using var context = await connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Loopback, Port));
        Console.WriteLine("No-signal-wait scenario: client connected with Kestrel IConnectionFactory.");

        var handshakeConnection = CreateKestrelConnection(context, loggerFactory, "client-no-signal-handshake-wrapper");
        await RunClientHandshakeAndDetachAsync(context, handshakeConnection);

        Console.WriteLine("Client starts raw Kestrel data transfer immediately after client DetachAsync; no in-process server detach signal is awaited.");
        await VerifyRawDataAfterDetachAsync(context);

        Console.WriteLine("No-signal-wait Kestrel raw handoff matched exactly; no byte shift or corruption was observed.");
        Console.WriteLine("Demo complete.");
    }

    private static async Task RunEarlyRawDataDuringDetachDemoAsync(IServiceProvider services)
    {
        _serverDetachDelay = TimeSpan.FromMilliseconds(250);
        ResetServerHandshakeDetachedSignal();

        var connectionFactory = services.GetRequiredService<Microsoft.AspNetCore.Connections.IConnectionFactory>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        await using var context = await connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Loopback, Port));
        Console.WriteLine("Early-data scenario: client connected with Kestrel IConnectionFactory.");

        var handshakeConnection = CreateKestrelConnection(context, loggerFactory, "client-early-data-handshake-wrapper");
        await RunClientHandshakeAndDetachAsync(context, handshakeConnection);

        Console.WriteLine("Kestrel client writes raw data after client DetachAsync, without waiting for the server-side detach signal.");
        await WriteRawMessageAsync(context.Transport.Output, "raw-arrived-before-server-detach");

        const string expected = "raw-echo:raw-arrived-before-server-detach";

        try
        {
            var response = await ReadRawMessageAsync(context.Transport.Input, context.ConnectionClosed).WaitAsync(TimeSpan.FromSeconds(2));
            Console.WriteLine($"Client received early raw response: {response}");

            if (response.Equals(expected, StringComparison.Ordinal))
                throw new InvalidOperationException("The unsafe early-raw scenario unexpectedly succeeded; this should reproduce detach-time data corruption.");

            throw new InvalidOperationException($"Unexpected early raw response. Expected the unsafe scenario to fail, but got '{response}'.");
        }
        catch (TimeoutException)
        {
            await WaitForServerHandshakeDetachedAsync();
            Console.WriteLine("Expected unsafe case reproduced: early raw data was consumed by the handshake wrapper, so no valid raw echo was produced.");
        }
        catch (EndOfStreamException)
        {
            await WaitForServerHandshakeDetachedAsync();
            Console.WriteLine("Expected unsafe case reproduced: the raw stream ended after detach-time data corruption was detected.");
        }

        Console.WriteLine("Demo complete.");
    }

    private static async Task RunTcpEarlyRawComparisonDemoAsync(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, TcpPort));
        listener.Listen(backlog: 1);

        var serverTask = TcpEarlyRawComparison.RunTcpEarlyRawServerAsync(listener, loggerFactory);

        using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, TcpPort));
        Console.WriteLine("TCP comparison scenario: client connected with Socket + TcpPipeConnection.");

        var handshakeConnection = new TcpPipeConnection(clientSocket, new ConnectionOptions
        {
            Logger = loggerFactory.CreateLogger("tcp-client-handshake-wrapper"),
            ReadAsDemand = false
        });

        var settingsAckReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readTask = ReadClientHandshakeFramesAsync(handshakeConnection, settingsAckReceived);

        await handshakeConnection.SendAsync(ClientPreface.AsMemory());
        await handshakeConnection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.None, 0, []).AsMemory());
        await settingsAckReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Console.WriteLine("TCP client detaches handshake wrapper before writing raw data.");
        await handshakeConnection.DetachAsync();
        await readTask.WaitAsync(TimeSpan.FromSeconds(5));

        const string message = "tcp-raw-arrived-before-server-detach";
        Console.WriteLine("TCP client writes raw data after client DetachAsync, without waiting for an in-process server detach signal.");
        await WriteRawMessageAsync(clientSocket, message);

        var response = await ReadRawMessageAsync(clientSocket, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
        var expected = "raw-echo:" + message;
        Console.WriteLine($"TCP client received raw response: {response}");

        if (!response.Equals(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"TCP raw data mismatch. Expected '{expected}', got '{response}'.");

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        Console.WriteLine("TCP comparison succeeded without waiting for an in-process server detach signal.");
        Console.WriteLine("Demo complete.");
    }

    private static KestrelPipeConnection CreateKestrelConnection(ConnectionContext context, ILoggerFactory loggerFactory, string loggerName)
    {
        return new DetachableKestrelPipeConnection(context, new ConnectionOptions
        {
            Logger = loggerFactory.CreateLogger(loggerName),
            ReadAsDemand = false
        });
    }

    private static async Task RunClientHandshakeAndDetachAsync(ConnectionContext context, IConnection connection)
    {
        var settingsAckReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readTask = ReadClientHandshakeFramesAsync(connection, settingsAckReceived);

        try
        {
            await connection.SendAsync(ClientPreface.AsMemory());
            await connection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.None, 0, []).AsMemory());
            Console.WriteLine("Client sent HTTP/2 connection preface and SETTINGS through SuperSocket.Kestrel.");

            await settingsAckReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            Console.WriteLine("Client handshake complete; detaching SuperSocket connection wrapper...");
            await connection.DetachAsync();
            await readTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    private static async Task ReadClientHandshakeFramesAsync(
        IConnection connection,
        TaskCompletionSource settingsAckReceived)
    {
        try
        {
            await foreach (var frame in connection.RunAsync(new Http2PipelineFilter(expectClientPreface: false)))
            {
                if (frame.Type == Http2FrameType.Settings)
                {
                    Console.WriteLine(frame.IsSettingsAck
                        ? "Client received server SETTINGS ACK."
                        : "Client received server SETTINGS.");

                    if (frame.IsSettingsAck)
                        settingsAckReceived.TrySetResult();

                    continue;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // DetachAsync cancels the reader to stop this wrapper without closing the Kestrel transport.
        }
        catch (Exception ex)
        {
            settingsAckReceived.TrySetException(ex);
        }
    }

    private static async Task VerifyRawDataAfterDetachAsync(ConnectionContext context)
    {
        var messages = new[]
        {
            "normal-data-1",
            "normal-data-after-detach-2",
            "normal-data-with-unicode-汉字-🙂"
        };

        foreach (var message in messages)
        {
            await WriteRawMessageAsync(context.Transport.Output, message);
            var response = await ReadRawMessageAsync(context.Transport.Input, context.ConnectionClosed);
            var expected = "raw-echo:" + message;

            Console.WriteLine($"Client received raw response: {response}");

            if (!response.Equals(expected, StringComparison.Ordinal))
                throw new InvalidOperationException($"Raw data mismatch. Expected '{expected}', got '{response}'.");
        }
    }

    internal static async Task WriteRawMessageAsync(PipeWriter writer, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var packet = new byte[sizeof(int) + payload.Length];
        WriteInt32BigEndian(packet.AsSpan(0, sizeof(int)), payload.Length);
        payload.CopyTo(packet.AsSpan(sizeof(int)));
        await writer.WriteAsync(packet.AsMemory());
    }

    private static async Task WriteRawMessageAsync(Socket socket, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var packet = new byte[sizeof(int) + payload.Length];
        WriteInt32BigEndian(packet.AsSpan(0, sizeof(int)), payload.Length);
        payload.CopyTo(packet.AsSpan(sizeof(int)));
        await SendAllAsync(socket, packet, CancellationToken.None);
    }

    internal static async Task<string> ReadRawMessageAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            try
            {
                if (TryReadRawMessage(buffer, out var message, out var consumed))
                {
                    reader.AdvanceTo(consumed);
                    return message;
                }

                if (result.IsCompleted)
                    throw new EndOfStreamException("The transport ended before a complete raw message was received.");

                reader.AdvanceTo(buffer.Start, buffer.End);
            }
            catch
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
                throw;
            }
        }
    }

    private static async Task<string> ReadRawMessageAsync(Socket socket, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[sizeof(int)];
        await ReceiveExactAsync(socket, lengthBuffer, cancellationToken);
        var length = ReadInt32BigEndian(lengthBuffer);

        if (length is < 0 or > 1024)
            throw new ProtocolException($"Invalid raw message length {length}.");

        var payload = new byte[length];
        await ReceiveExactAsync(socket, payload, cancellationToken);
        return Encoding.UTF8.GetString(payload);
    }

    private static async Task SendAllAsync(Socket socket, ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
    {
        while (!packet.IsEmpty)
        {
            var sent = await socket.SendAsync(packet, SocketFlags.None, cancellationToken);

            if (sent <= 0)
                throw new IOException("Socket send completed without sending data.");

            packet = packet.Slice(sent);
        }
    }

    private static async Task ReceiveExactAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        while (!buffer.IsEmpty)
        {
            var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

            if (received == 0)
                throw new EndOfStreamException("Socket closed before a complete raw message was received.");

            buffer = buffer.Slice(received);
        }
    }

    private static bool TryReadRawMessage(ReadOnlySequence<byte> buffer, out string message, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(buffer);

        if (!reader.TryReadBigEndian(out int length))
        {
            message = string.Empty;
            consumed = buffer.Start;
            return false;
        }

        if (length is < 0 or > 1024)
            throw new ProtocolException($"Invalid raw message length {length}.");

        if (reader.Remaining < length)
        {
            message = string.Empty;
            consumed = buffer.Start;
            return false;
        }

        message = Encoding.UTF8.GetString(reader.Sequence.Slice(reader.Position, length));
        reader.Advance(length);
        consumed = reader.Position;
        return true;
    }

    private static void WriteInt32BigEndian(Span<byte> destination, int value)
    {
        destination[0] = (byte)((value >> 24) & 0xff);
        destination[1] = (byte)((value >> 16) & 0xff);
        destination[2] = (byte)((value >> 8) & 0xff);
        destination[3] = (byte)(value & 0xff);
    }

    private static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return (source[0] << 24) | (source[1] << 16) | (source[2] << 8) | source[3];
    }

    internal static TimeSpan ServerDetachDelay => _serverDetachDelay;

    internal static void NotifyServerHandshakeDetached()
    {
        _serverHandshakeDetached.TrySetResult();
    }

    private static TaskCompletionSource CreateSignal()
    {
        return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static void ResetServerHandshakeDetachedSignal()
    {
        _serverHandshakeDetached = CreateSignal();
    }

    private static async Task WaitForServerHandshakeDetachedAsync()
    {
        await _serverHandshakeDetached.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    internal static void IncrementActiveServerConnections()
    {
        Interlocked.Increment(ref _activeServerConnections);
    }

    internal static void DecrementActiveServerConnections()
    {
        Interlocked.Decrement(ref _activeServerConnections);
    }
}

internal sealed class DetachableKestrelPipeConnection(ConnectionContext context, ConnectionOptions options)
    : KestrelPipeConnection(context, options)
{
    // Demo-only wrapper: after DetachAsync the ASP.NET Core ConnectionContext keeps owning the transport pipes.
    protected override ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
    {
        return ValueTask.CompletedTask;
    }
}

internal sealed class Http2ConnectionHandler(ILoggerFactory loggerFactory) : ConnectionHandler
{
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        Program.IncrementActiveServerConnections();

        var connection = new DetachableKestrelPipeConnection(context, new ConnectionOptions
        {
            Logger = loggerFactory.CreateLogger<Http2ConnectionHandler>(),
            ReadAsDemand = false
        });

        try
        {
            await RunServerHandshakeAndDetachAsync(connection);
            await RunRawEchoAsync(context);
        }
        finally
        {
            Program.DecrementActiveServerConnections();
        }
    }

    private static async Task RunServerHandshakeAndDetachAsync(IConnection connection)
    {
        var settingsReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readTask = ReadServerHandshakeFramesAsync(connection, settingsReceived);

        try
        {
            await settingsReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

            if (Program.ServerDetachDelay > TimeSpan.Zero)
                await Task.Delay(Program.ServerDetachDelay);
        }
        finally
        {
            Console.WriteLine("Server handshake complete; detaching SuperSocket connection wrapper...");
            await connection.DetachAsync();
            await readTask.WaitAsync(TimeSpan.FromSeconds(5));
            Program.NotifyServerHandshakeDetached();
        }
    }

    private static async Task ReadServerHandshakeFramesAsync(IConnection connection, TaskCompletionSource settingsReceived)
    {
        try
        {
            await foreach (var frame in connection.RunAsync(new Http2PipelineFilter()))
            {
                Console.WriteLine($"Server received handshake {frame.Type} frame on stream {frame.StreamId}.");

                if (frame.Type == Http2FrameType.Settings && !frame.IsSettingsAck)
                {
                    await connection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.None, 0, []).AsMemory());
                    await connection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.Ack, 0, []).AsMemory());
                    settingsReceived.TrySetResult();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // DetachAsync cancels the SuperSocket reader; raw Kestrel transport remains open.
        }
        catch (Exception ex)
        {
            settingsReceived.TrySetException(ex);
        }
    }

    private static async Task RunRawEchoAsync(ConnectionContext context)
    {
        while (!context.ConnectionClosed.IsCancellationRequested)
        {
            string message;

            try
            {
                message = await Program.ReadRawMessageAsync(context.Transport.Input, context.ConnectionClosed);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ProtocolException ex)
            {
                Console.WriteLine($"Server detected corrupted raw framing after detach: {ex.Message}");
                return;
            }

            Console.WriteLine($"Server received raw data after detach: {message}");
            await Program.WriteRawMessageAsync(context.Transport.Output, "raw-echo:" + message);
        }
    }
}

internal static class TcpEarlyRawComparison
{
    public static async Task RunTcpEarlyRawServerAsync(Socket listener, ILoggerFactory loggerFactory)
    {
        using var serverSocket = await listener.AcceptAsync();
        var connection = new TcpPipeConnection(serverSocket, new ConnectionOptions
        {
            Logger = loggerFactory.CreateLogger("tcp-server-handshake-wrapper"),
            ReadAsDemand = false
        });

        var settingsReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readTask = ReadServerHandshakeFramesAsync(connection, settingsReceived);

        try
        {
            await settingsReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            Console.WriteLine("TCP server detaches handshake wrapper immediately after SETTINGS ACK.");
            await connection.DetachAsync();
            await readTask.WaitAsync(TimeSpan.FromSeconds(5));
        }

        var message = await ReadRawMessageAsync(serverSocket, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
        Console.WriteLine($"TCP server received raw data after detach: {message}");
        await WriteRawMessageAsync(serverSocket, "raw-echo:" + message);
    }

    private static async Task WriteRawMessageAsync(Socket socket, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var packet = new byte[sizeof(int) + payload.Length];
        WriteInt32BigEndian(packet.AsSpan(0, sizeof(int)), payload.Length);
        payload.CopyTo(packet.AsSpan(sizeof(int)));
        await SendAllAsync(socket, packet, CancellationToken.None);
    }

    private static async Task<string> ReadRawMessageAsync(Socket socket, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[sizeof(int)];
        await ReceiveExactAsync(socket, lengthBuffer, cancellationToken);
        var length = ReadInt32BigEndian(lengthBuffer);

        if (length is < 0 or > 1024)
            throw new ProtocolException($"Invalid raw message length {length}.");

        var payload = new byte[length];
        await ReceiveExactAsync(socket, payload, cancellationToken);
        return Encoding.UTF8.GetString(payload);
    }

    private static async Task SendAllAsync(Socket socket, ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
    {
        while (!packet.IsEmpty)
        {
            var sent = await socket.SendAsync(packet, SocketFlags.None, cancellationToken);

            if (sent <= 0)
                throw new IOException("Socket send completed without sending data.");

            packet = packet.Slice(sent);
        }
    }

    private static async Task ReceiveExactAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        while (!buffer.IsEmpty)
        {
            var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

            if (received == 0)
                throw new EndOfStreamException("Socket closed before a complete raw message was received.");

            buffer = buffer.Slice(received);
        }
    }

    private static void WriteInt32BigEndian(Span<byte> destination, int value)
    {
        destination[0] = (byte)((value >> 24) & 0xff);
        destination[1] = (byte)((value >> 16) & 0xff);
        destination[2] = (byte)((value >> 8) & 0xff);
        destination[3] = (byte)(value & 0xff);
    }

    private static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return (source[0] << 24) | (source[1] << 16) | (source[2] << 8) | source[3];
    }

    private static async Task ReadServerHandshakeFramesAsync(IConnection connection, TaskCompletionSource settingsReceived)
    {
        try
        {
            await foreach (var frame in connection.RunAsync(new Http2PipelineFilter()))
            {
                Console.WriteLine($"TCP server received handshake {frame.Type} frame on stream {frame.StreamId}.");

                if (frame.Type == Http2FrameType.Settings && !frame.IsSettingsAck)
                {
                    await connection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.None, 0, []).AsMemory());
                    await connection.SendAsync(Http2Frame.Write(Http2FrameType.Settings, (byte)Http2FrameFlags.Ack, 0, []).AsMemory());
                    settingsReceived.TrySetResult();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // DetachAsync cancels the SuperSocket reader; the socket remains open for raw reads.
        }
        catch (Exception ex)
        {
            settingsReceived.TrySetException(ex);
        }
    }
}
