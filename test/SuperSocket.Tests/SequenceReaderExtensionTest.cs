using System;
using System.Buffers;
using System.Buffers.Binary;
using SuperSocket.ProtoBase;
using Xunit;

namespace SuperSocket.Tests
{
    public class SequenceReaderExtensionTest
    {
        [Fact]
        public void TestReadBigEndianUShort()
        {
            var arr = new byte[4];
            Span<byte> span = arr;

            ushort value1 = ushort.MaxValue;
            ushort value2 = ushort.MinValue;
            
            BinaryPrimitives.WriteUInt16BigEndian(span, value1);
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), value2);

            var sequence = new ReadOnlySequence<byte>(arr);            
            var reader = new SequenceReader<byte>(sequence);

            Assert.True(reader.TryReadBigEndian(out ushort result1));
            Assert.Equal(value1, result1);

            Assert.True(reader.TryReadBigEndian(out ushort result2));
            Assert.Equal(value2, result2);
        }

        [Fact]
        public void TestReadBigEndianUInt()
        {
            var arr = new byte[8];
            Span<byte> span = arr;

            uint value1 = uint.MaxValue;
            uint value2 = uint.MinValue;
            
            BinaryPrimitives.WriteUInt32BigEndian(span, value1);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(4), value2);

            var sequence = new ReadOnlySequence<byte>(arr);            
            var reader = new SequenceReader<byte>(sequence);

            Assert.True(reader.TryReadBigEndian(out uint result1));
            Assert.Equal(value1, result1);

            Assert.True(reader.TryReadBigEndian(out uint result2));
            Assert.Equal(value2, result2);
        }

        [Fact]
        public void TestReadBigEndianULong()
        {
            var arr = new byte[16];
            Span<byte> span = arr;

            ulong value1 = ulong.MaxValue;
            ulong value2 = ulong.MinValue;
            
            BinaryPrimitives.WriteUInt64BigEndian(span, value1);
            BinaryPrimitives.WriteUInt64BigEndian(span.Slice(8), value2);

            var sequence = new ReadOnlySequence<byte>(arr);            
            var reader = new SequenceReader<byte>(sequence);

            Assert.True(reader.TryReadBigEndian(out ulong result1));
            Assert.Equal(value1, result1);

            Assert.True(reader.TryReadBigEndian(out ulong result2));
            Assert.Equal(value2, result2);
        }
    }
}