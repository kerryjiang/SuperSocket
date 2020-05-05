using System;
using System.Buffers;
using System.Text;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace CustomProtocol
{
    public class MyPackageFilter : FixedHeaderPipelineFilter<MyPackage>
    {
        /// <summary>
        /// Header size is 5
        /// 1: OpCode
        /// 2-3: body length
        /// 4-5: sequence
        /// </summary>
        public MyPackageFilter()
            : base(5)
        {

        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.Advance(1); // skip the first byte for OpCode
            reader.TryReadBigEndian(out short len);
            reader.Advance(2); // skip the two bytes for Sequence

            return len;
        }

        protected override MyPackage DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var package = new MyPackage();

            var reader = new SequenceReader<byte>(buffer);
            
            reader.TryRead(out byte opCodeByte);
            package.Code = (OpCode)opCodeByte;

             // skip the two bytes for length, we don't need length any more
             // because we already get the full data of the package in the buffer
            reader.Advance(2);

            reader.TryReadBigEndian(out short sequence);
            package.Sequence = sequence;
            // get the rest of the data in the reader and then read it as utf8 string
            package.Body = reader.Sequence.Slice(reader.Consumed).GetString(Encoding.UTF8);

            return package;
        }
    }
}