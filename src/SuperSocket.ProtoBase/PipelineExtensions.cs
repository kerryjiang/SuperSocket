using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public static class PipelineExtensions
    {
        public static bool TrySliceTo(this ReadOnlySequence<byte> buffer, Span<byte> tag, out ReadOnlySequence<byte> slice, out SequencePosition position)
        {
            throw new NotImplementedException();
        }

        public static string GetUtf8String(this ReadOnlySequence<byte> buffer)
        {
            throw new NotImplementedException();
        }
    }
}