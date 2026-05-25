using System.Buffers;
using System.Text;

namespace KestrelHttp2DetachDemo;

internal enum Http2FrameType : byte
{
    Data = 0x0,
    Settings = 0x4
}

internal enum Http2FrameFlags : byte
{
    None = 0x0,
    Ack = 0x1,
    EndStream = 0x1
}

internal sealed class Http2Frame
{
    public const int HeaderLength = 9;

    public Http2Frame(int length, Http2FrameType type, byte flags, int streamId, byte[] payload)
    {
        Length = length;
        Type = type;
        Flags = flags;
        StreamId = streamId;
        Payload = payload;
    }

    public int Length { get; }

    public Http2FrameType Type { get; }

    public byte Flags { get; }

    public int StreamId { get; }

    public byte[] Payload { get; }

    public bool IsSettingsAck => Type == Http2FrameType.Settings && (Flags & (byte)Http2FrameFlags.Ack) != 0;

    public string PayloadText => Encoding.UTF8.GetString(Payload);

    public static Http2Frame Read(ReadOnlySequence<byte> buffer)
    {
        Span<byte> header = stackalloc byte[HeaderLength];
        buffer.Slice(0, HeaderLength).CopyTo(header);

        var length = ReadUInt24(header);
        var type = (Http2FrameType)header[3];
        var flags = header[4];
        var streamId = ReadInt31(header.Slice(5, 4));
        var payload = buffer.Slice(HeaderLength, length).ToArray();

        return new Http2Frame(length, type, flags, streamId, payload);
    }

    public static byte[] Write(Http2FrameType type, byte flags, int streamId, ReadOnlySpan<byte> payload)
    {
        var frame = new byte[HeaderLength + payload.Length];
        WriteUInt24(frame, payload.Length);
        frame[3] = (byte)type;
        frame[4] = flags;
        WriteInt31(frame.AsSpan(5, 4), streamId);
        payload.CopyTo(frame.AsSpan(HeaderLength));
        return frame;
    }

    private static int ReadUInt24(ReadOnlySpan<byte> source)
    {
        return (source[0] << 16) | (source[1] << 8) | source[2];
    }

    private static void WriteUInt24(Span<byte> destination, int value)
    {
        destination[0] = (byte)((value >> 16) & 0xff);
        destination[1] = (byte)((value >> 8) & 0xff);
        destination[2] = (byte)(value & 0xff);
    }

    private static int ReadInt31(ReadOnlySpan<byte> source)
    {
        return ((source[0] & 0x7f) << 24) | (source[1] << 16) | (source[2] << 8) | source[3];
    }

    private static void WriteInt31(Span<byte> destination, int value)
    {
        destination[0] = (byte)((value >> 24) & 0x7f);
        destination[1] = (byte)((value >> 16) & 0xff);
        destination[2] = (byte)((value >> 8) & 0xff);
        destination[3] = (byte)(value & 0xff);
    }
}
