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
        public void ReadByte()
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
        public void ReadByte_should_advance_position()
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
        public void Take_should()
        {
            // LINQ allows you to skip more than the size...
            var x = Enumerable.Range(1, 10).Skip(1).Skip(11).ToArray();
            Assert.AreEqual(0, x.Length);
        }

        [Test]
        public void ReadString_from_one_segment()
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
        public void ReadString_using_ASCII_from_two_segments()
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
        public void ReadString_from_multiple_segments()
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
        //[Ignore]
        public void Skip_one_and_ReadString_from_two_segments()
        {
            string expected = "The quick brown fox jumps over the lazy dog";
            Encoding encoding = Encoding.ASCII;

            List<ArraySegment<byte>> segments = GetSegments(expected, encoding, 2);

            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            // pre-condition
            Assert.AreEqual(expected.Length, sut.Length);

            // act
            var actual = sut.Skip(1).ReadString(expected.Length - 1, encoding);

            Assert.AreEqual(expected.Substring(1), actual);
            Assert.AreEqual(expected.Length, sut.Position);
        }

        [Test]
        public void Skip_should()
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
        public void ReadInt16(bool littleEndian, byte[] bytes, short expected)
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
        public void ReadUInt16(bool littleEndian, byte[] bytes, ushort expected)
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
        public void ReadInt32(bool littleEndian, byte[] bytes, long expected)
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
        public void ReadUInt32(bool littleEndian, byte[] bytes, ulong expected)
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
        public void ReadInt64(bool littleEndian, byte[] bytes, long expected)
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
        public void ReadUInt64(bool littleEndian, byte[] bytes, ulong expected)
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
