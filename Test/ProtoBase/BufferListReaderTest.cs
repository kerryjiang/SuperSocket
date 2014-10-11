using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test.ProtoBase
{
    [TestFixture]
    public class BufferListReaderTest
    {
        [Test]
        public void should_be_able_to_read_a_single_byte()
        {
            byte expected = 0x01;

            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { GetSegment(new byte[] { expected }, 123, 1024) });

            // pre-condition
            Assert.AreEqual(1, sut.Length);
            Assert.AreEqual(0, sut.Position);
            
            // act
            var actual = sut.ReadByte();

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(1, sut.Position);
        }

        [Test]
        public void should_be_able_to_read_a_byte_and_the_position_should_be_advanced_by_one()
        {
            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { GetSegment(new byte[] { 0x01, 0x02 }, 512, 1024) });

            // pre-condition
            Assert.AreEqual(2, sut.Length);
            Assert.AreEqual(0, sut.Position);

            // act
            byte first = sut.ReadByte();

            // assert
            Assert.AreEqual(0x01, first);
            Assert.AreEqual(1, sut.Position);
        }


        [Test]
        public void should_be_able_to_take_a_buffer_from_the_current_position()
        {
            BufferListReader sut = new BufferListReader();
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            for (int i = 0; i < 10; i++)
            {
                segments.Add(GetSegment(new byte[16], 512, 1024));
            }
            sut.Initialize(segments);

            // pre-condition
            Assert.AreEqual(0, sut.Position);

            sut.Position += 24;

            // act
            var actual = sut.Take(36);

            var totalLength = actual.Sum(segment => segment.Count);

            // assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(36, totalLength);
            // TODO: check if the data is correct
        }

        [Test]
        public void should_be_able_to_advance_the_position_within_current_segment()
        {
            int segmentLength = 16;
            int expectedPosition = segmentLength/2;

            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { GetSegment(new byte[segmentLength], 512, 1024) });

            Assert.AreEqual(0, sut.Position);

            sut.Position += expectedPosition;

            Assert.AreEqual(expectedPosition, sut.Position);
        }

        [Test]
        public void should_be_able_to_advance_the_position_into_the_next_segment()
        {
            BufferListReader sut = new BufferListReader();
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(GetSegment(new byte[16], 512, 1024));
            segments.Add(GetSegment(new byte[16], 512, 1024));
            sut.Initialize(segments);

            Assert.AreEqual(0, sut.Position);

            sut.Position += 24;

            Assert.AreEqual(24, sut.Position);
        }

        [Test]
        public void should_be_able_to_rewind_the_position_into_the_previous_segment()
        {
            int segmentLength = 16;
            int expectedPosition = segmentLength * 3 / 2;

            BufferListReader sut = new BufferListReader();
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(GetSegment(new byte[segmentLength], 512, 1024));
            segments.Add(GetSegment(new byte[segmentLength], 512, 1024));
            sut.Initialize(segments);

            Assert.AreEqual(0, sut.Position);

            sut.Position += expectedPosition;

            Assert.AreEqual(expectedPosition, sut.Position);

            sut.Position -= segmentLength;
            Assert.AreEqual(segmentLength/2, sut.Position);
        }

        [Test]
        public void should_be_able_to_read_a_string_from_one_segment()
        {
            string expected = "The quick brown fox jumps over the lazy dog";
            var encoding = Encoding.ASCII;

            var bytes = encoding.GetBytes(expected);

            ArraySegment<byte> segment = new ArraySegment<byte>(bytes);

            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { segment });

            // act
            var actual = sut.ReadString(expected.Length, encoding);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Length, sut.Position);
        }

        [Test]
        public void should_be_able_to_read_a_string_from_segments()
        {
            string expected = "The quick brown fox jumps over the lazy dog";

            BufferListReader sut = new BufferListReader();
            sut.Initialize(GetSegments(expected, Encoding.ASCII, 2));

            // pre-condition
            Assert.AreEqual(expected.Length, sut.Length);

            var actual = sut.ReadString(expected.Length, Encoding.ASCII);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Length, sut.Position);
        }

        [Test]
        public void should_be_able_to_read_a_string_from_multiple_segments()
        {
            string expected = "The quick brown fox jumps over the lazy dog";
            Encoding encoding = Encoding.ASCII;

            // create n-segments of each word and space
            Random random = new Random(0);

            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            var words = expected.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (i != 0)
                {
                    segments.Add(GetSegment(encoding.GetBytes(" "), random.Next(0,256), random.Next(1, 4) * 1024));
                }

                segments.Add(GetSegment(encoding.GetBytes(words[i]), random.Next(0, 256), random.Next(1, 4) * 1024));
            }

            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // pre-condition
            Assert.AreEqual(expected.Length, sut.Length);

            string actual = sut.ReadString(expected.Length, encoding);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Length, sut.Position);
        }

        [Test]
        public void should_be_able_to_call_skip_to_advance_the_position()
        {
            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { GetSegment(new byte[] { 0x01 }, 123, 1024) });

            // pre-condition
            Assert.AreEqual(1, sut.Length);

            // act
            var actual = sut.Skip(1);
            
            // assert
            Assert.NotNull(actual);
            Assert.AreEqual(1, sut.Position);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void should_not_be_able_to_skip_backwards()
        {
            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { GetSegment(new byte[] { 0x01, 0x02, 0x03, 0x04 }, 123, 1024) });
            sut.Position = 1;
            sut.Skip(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void should_not_be_able_to_skip_past_the_end_of_all_the_buffers()
        {
            int segmentSize = 16;
            BufferListReader sut = new BufferListReader();
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(GetSegment(new byte[segmentSize], 512, 1024));
            segments.Add(GetSegment(new byte[segmentSize], 512, 1024));
            sut.Initialize(segments);

            sut.Skip(segmentSize * 2 + 1);
        }

        [Test]
        public void should_be_able_to_skip_to_the_end_of_all_the_buffers()
        {
            int segmentSize = 16;
            BufferListReader sut = new BufferListReader();
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(GetSegment(new byte[segmentSize], 512, 1024));
            segments.Add(GetSegment(new byte[segmentSize], 512, 1024));
            sut.Initialize(segments);

            sut.Skip(segmentSize * 2);
        }

        [TestCase(true, new byte[] { 0, 0 }, (short)0)]
        [TestCase(true, new byte[] { 1, 0 }, (short)1)]
        [TestCase(true, new byte[] { 0, 1 }, (short)256)]
        [TestCase(true, new byte[] { 1, 1 }, (short)257)]
        [TestCase(true, new byte[] { 255, 255 }, (short)-1)]
        [TestCase(false, new byte[] { 0, 0 }, (short)0)]
        [TestCase(false, new byte[] { 0, 1 }, (short)1)]
        [TestCase(false, new byte[] { 1, 0 }, (short)256)]
        [TestCase(false, new byte[] { 1, 1 }, (short)257)]
        [TestCase(false, new byte[] { 255, 255, }, (short)-1)]
        public void should_be_able_to_read_a_short_integer(bool littleEndian, byte[] bytes, short expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            var actual = sut.ReadInt16(littleEndian);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(2, sut.Position);
        }

        [TestCase(true, new byte[] { 0, 0 }, (ushort)0)]
        [TestCase(true, new byte[] { 1, 0 }, (ushort)1)]
        [TestCase(true, new byte[] { 0, 1 }, (ushort)256)]
        [TestCase(true, new byte[] { 1, 1 }, (ushort)257)]
        [TestCase(true, new byte[] { 255, 255 }, (ushort)ushort.MaxValue)]
        [TestCase(false, new byte[] { 0, 0 }, (ushort)0)]
        [TestCase(false, new byte[] { 0, 1 }, (ushort)1)]
        [TestCase(false, new byte[] { 1, 0 }, (ushort)256)]
        [TestCase(false, new byte[] { 1, 1 }, (ushort)257)]
        [TestCase(false, new byte[] { 255, 255, }, (ushort)ushort.MaxValue)]
        public void should_be_able_to_read_an_unsigned_short_integer(bool littleEndian, byte[] bytes, ushort expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            var actual = sut.ReadUInt16(littleEndian);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(2, sut.Position);
        }

        [TestCase(true, new byte[] { 0, 0, 0, 0 }, 0L)]
        [TestCase(true, new byte[] { 1, 0, 0, 0 }, 1L)]
        [TestCase(true, new byte[] { 0, 1, 0, 0 }, 256L)]
        [TestCase(true, new byte[] { 0, 0, 1, 0 }, 65536L)]
        [TestCase(true, new byte[] { 0, 0, 0, 1 }, 16777216L)]
        [TestCase(true, new byte[] { 255, 255, 255, 255 }, -1L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0 }, 0L)]
        [TestCase(false, new byte[] { 0, 0, 0, 1 }, 1L)]
        [TestCase(false, new byte[] { 0, 0, 1, 0 }, 256L)]
        [TestCase(false, new byte[] { 0, 1, 0, 0 }, 65536L)]
        [TestCase(false, new byte[] { 1, 0, 0, 0 }, 16777216L)]
        [TestCase(false, new byte[] { 255, 255, 255, 255 }, -1L)]
        public void should_be_able_to_read_an_integer(bool littleEndian, byte[] bytes, long expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            var actual = sut.ReadInt32(littleEndian);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(4, sut.Position);
        }

        [TestCase(true, new byte[] { 0, 0, 0, 0 }, 0UL)]
        [TestCase(true, new byte[] { 1, 0, 0, 0 }, 1UL)]
        [TestCase(true, new byte[] { 0, 1, 0, 0 }, 256UL)]
        [TestCase(true, new byte[] { 0, 0, 1, 0 }, 65536UL)]
        [TestCase(true, new byte[] { 0, 0, 0, 1 }, 16777216UL)]
        [TestCase(true, new byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
        [TestCase(false, new byte[] { 0, 0, 0, 0 }, 0UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 1 }, 1UL)]
        [TestCase(false, new byte[] { 0, 0, 1, 0 }, 256UL)]
        [TestCase(false, new byte[] { 0, 1, 0, 0 }, 65536UL)]
        [TestCase(false, new byte[] { 1, 0, 0, 0 }, 16777216UL)]
        [TestCase(false, new byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
        public void should_be_able_to_read_an_unsigned_integer(bool littleEndian, byte[] bytes, ulong expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            var actual = sut.ReadUInt32(littleEndian);

            // assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(4, sut.Position);
        }

        [TestCase(true,  new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0L)]
        [TestCase(true,  new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1L)]
        [TestCase(true,  new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 256L)]
        [TestCase(true,  new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 65536L)]
        [TestCase(true,  new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 16777216L)]
        [TestCase(true,  new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 4294967296L)]
        [TestCase(true,  new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 1099511627776L)]
        [TestCase(true,  new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 1099511627776L * 256)]
        [TestCase(true,  new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1099511627776L * 256 * 256)]
        [TestCase(true, new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, -1L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 256L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 65536L)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 16777216L)]
        [TestCase(false, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 4294967296L)]
        [TestCase(false, new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 1099511627776L)]
        [TestCase(false, new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 1099511627776L * 256)]
        [TestCase(false, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1099511627776L * 256 * 256)]
        [TestCase(false, new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, -1L)]
        public void should_be_able_to_read_a_long_integer(bool littleEndian, byte[] bytes, long expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            long actual = sut.ReadInt64(littleEndian);

            // assert
            Assert.AreEqual(8, bytes.Length);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(8, sut.Position);
        }

        [TestCase(true, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0UL)]
        [TestCase(true, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1UL)]
        [TestCase(true, new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 256UL)]
        [TestCase(true, new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 65536UL)]
        [TestCase(true, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 16777216UL)]
        [TestCase(true, new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 4294967296UL)]
        [TestCase(true, new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 1099511627776UL)]
        [TestCase(true, new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 1099511627776UL * 256)]
        [TestCase(true, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1099511627776UL * 256 * 256)]
        [TestCase(true, new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, ulong.MaxValue)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 256UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 65536UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 16777216UL)]
        [TestCase(false, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 4294967296UL)]
        [TestCase(false, new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 1099511627776UL)]
        [TestCase(false, new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 1099511627776UL * 256)]
        [TestCase(false, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1099511627776UL * 256 * 256)]
        [TestCase(false, new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, ulong.MaxValue)]
        public void should_be_able_to_read_an_unsigned_long_integer(bool littleEndian, byte[] bytes, ulong expected)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();
            segments.Add(new ArraySegment<byte>(bytes));
            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // act
            ulong actual = sut.ReadUInt64(littleEndian);

            // assert
            Assert.AreEqual(8, bytes.Length);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(8, sut.Position);
        }

        /// <summary>
        /// Split the string value into <paramref name="count"/> pieces and a create list of <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<ArraySegment<byte>> GetSegments(string value, Encoding encoding, int count)
        {
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();

            int startIndex = 0;

            for (int i = 0; i < count; i++)
            {
                int length = value.Length / count;
                if (i == (count - 1))
                {
                    // last segment, consume whatever is left
                    length = value.Length - startIndex;
                }

                byte[] bytes = encoding.GetBytes(value.Substring(startIndex, length));
                startIndex += length;
                segments.Add(new ArraySegment<byte>(bytes));
            }

            return segments;
        }

        /// <summary>
        /// Gets the array segment with the user supplied data positioned at <paramref name="offset"/>.
        /// Random data is added around the user specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">size</exception>
        private ArraySegment<byte> GetSegment(byte[] data, int offset, int size)
        {
            if (size < data.Length + offset)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            // create a buffer with random data
            byte[] buffer = new byte[size];
            var random = new RNGCryptoServiceProvider();
            random.GetBytes(buffer);

            // copy the target data into the correct position
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);

            return new ArraySegment<byte>(buffer, offset, data.Length);
        }
    }
}
