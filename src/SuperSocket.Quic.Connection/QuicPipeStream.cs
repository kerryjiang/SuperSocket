using System;
using System.IO;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Quic.Connection;

#pragma warning disable CA2252

public sealed class QuicPipeStream : Stream
{
    private Stream _stream;

    private readonly bool _serverStream;
    private readonly QuicConnection _connection;

    public QuicPipeStream(QuicConnection connection, bool serverStream)
    {
        _connection = connection;
        _serverStream = serverStream;
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public async ValueTask OpenStreamAsync(CancellationToken cancellationToken)
    {
        if (_serverStream)
        {
            _stream = await _connection.AcceptInboundStreamAsync(cancellationToken);
        }
        else
        {
            _stream = await _connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken);
        }
    }

    public override void Flush() => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.Flush();

    public override void Close() => _stream.Close();

    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        _stream.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = default) => _stream.ReadAsync(buffer, cancellationToken);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        _stream.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default) =>
        _stream.WriteAsync(buffer, cancellationToken);

    public override void Write(ReadOnlySpan<byte> buffer) => _stream.Flush();

    public override void Write(byte[] buffer, int offset, int count) => _stream.Flush();
}