using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class SequenceSegment : ReadOnlySequenceSegment<byte>
    {
        public SequenceSegment(ReadOnlyMemory<byte> memory)
        {
            this.Memory = memory;
        }

        public SequenceSegment SetNext(ReadOnlyMemory<byte> nextMemory)
        {
            var segment = new SequenceSegment(nextMemory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }
    }
}
