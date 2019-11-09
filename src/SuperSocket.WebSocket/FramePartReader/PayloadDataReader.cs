using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (package.OpCode != OpCode.Text)
                package.Data = seq;
            else
                package.Message = seq.GetString(Encoding.UTF8);

            reader.Advance(required);
            
            return true;
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
