using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    class PayloadDataReader : DataFramePartReader
    {
        public override bool Process(WebSocketPackage package, ref SequenceReader<byte> reader, out IDataFramePartReader nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            long required = package.PayloadLength;

            if (reader.Remaining < required)
            {
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            var seq = reader.Sequence.Slice(reader.Consumed, required);

            if (package.HasMask)
                DecodeMask(seq, package.MaskKey);

            if (package.Data.Length == 0)
                package.Data = seq;
            else
                package.Data = ConcactSequence(package.Data, seq);

            try
            {
                if (package.FIN)
                {
                    if (package.OpCode == OpCode.Text)
                    {
                        package.Message = package.Data.GetString(Encoding.UTF8);
                        package.Data = default;
                    }

                    return true;
                }
                else
                {
                    // start to process next fragment
                    nextPartReader = FixPartReader;
                    return false;
                }
            }
            finally
            {
                reader.Advance(required);
            }
        }

        private ReadOnlySequence<byte> ConcactSequence(ReadOnlySequence<byte> first, ReadOnlySequence<byte> second)
        {
            SequenceSegment head = first.Start.GetObject() as SequenceSegment;
            SequenceSegment tail = first.End.GetObject() as SequenceSegment;
            
            if (head == null)
            {
                foreach (var segment in first)
                {                
                    if (head == null)
                        tail = head = new SequenceSegment(segment);
                    else
                        tail = tail.SetNext(segment);
                }
            }

            foreach (var segment in second)
            {
                tail = tail.SetNext(segment);
            }

            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        internal unsafe void DecodeMask(ReadOnlySequence<byte> sequence, byte[] mask)
        {
            var index = 0;
            var maskLen = mask.Length;

            foreach (var piece in sequence)
            {
                fixed (byte* ptr = &piece.Span.GetPinnableReference())
                {
                    var span = new Span<byte>(ptr, piece.Span.Length);

                    for (var i = 0; i < span.Length; i++)
                    {
                        span[i] = (byte)(span[i] ^ mask[index++ % maskLen]);
                    }
                }
            }
        }
    }
}
