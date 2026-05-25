using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace KestrelHttp2DetachDemo;

internal sealed class Http2PipelineFilter : PipelineFilterBase<Http2Frame>
{
    private static readonly byte[] ClientPreface = Encoding.ASCII.GetBytes("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n");

    private readonly bool _expectClientPreface;
    private bool _clientPrefaceRead;
    private bool _foundHeader;
    private int _totalSize;

    public Http2PipelineFilter(bool expectClientPreface = true)
    {
        _expectClientPreface = expectClientPreface;
    }

    public override Http2Frame Filter(ref SequenceReader<byte> reader)
    {
        if (_expectClientPreface && !_clientPrefaceRead)
        {
            if (reader.Remaining < ClientPreface.Length)
                return null!;

            Span<byte> preface = stackalloc byte[24];
            reader.Sequence.Slice(reader.Position, ClientPreface.Length).CopyTo(preface);

            if (!preface.SequenceEqual(ClientPreface))
                throw new ProtocolException("The client did not send a valid HTTP/2 connection preface.");

            reader.Advance(ClientPreface.Length);
            _clientPrefaceRead = true;
        }

        if (!_foundHeader)
        {
            if (reader.Remaining < Http2Frame.HeaderLength)
                return null!;

            var header = reader.Sequence.Slice(reader.Position, Http2Frame.HeaderLength);
            var bodyLength = GetBodyLengthFromHeader(header);

            if (bodyLength < 0)
                throw new ProtocolException("Failed to get body length from the HTTP/2 frame header.");

            _foundHeader = true;
            _totalSize = Http2Frame.HeaderLength + bodyLength;
        }

        if (reader.Remaining < _totalSize)
            return null!;

        var package = reader.Sequence.Slice(reader.Position, _totalSize);
        var frame = Http2Frame.Read(package);
        reader.Advance(_totalSize);
        _foundHeader = false;
        _totalSize = 0;

        return frame;
    }

    public override void Reset()
    {
        base.Reset();
        _foundHeader = false;
        _totalSize = 0;
    }

    private static int GetBodyLengthFromHeader(ReadOnlySequence<byte> headerBuffer)
    {
        Span<byte> header = stackalloc byte[3];
        headerBuffer.Slice(0, 3).CopyTo(header);
        return (header[0] << 16) | (header[1] << 8) | header[2];
    }
}
