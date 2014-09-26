using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test.ProtoBase
{
    [TestFixture]
    public class BufferListReaderTest
    {
        private const int BufferSize = 4096;

        [Test]
        public void Skip_should()
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(new byte[] { 0x01 });

            BufferListReader sut = new BufferListReader();
            sut.Initialize(new[] { segment });

            Assert.AreEqual(1, sut.Length);
            sut.Skip(1);
            
        }

        [Test]
        public void Take_should()
        {
            
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

            var actual = sut.ReadString(expected.Length, encoding);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReadString_from_two_segments()
        {
            string expected = "The quick brown fox jumps over the lazy dog";
            Encoding encoding = Encoding.ASCII;

            List<ArraySegment<byte>> segments = GetSegments(expected, encoding, 2);

            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            Assert.AreEqual(expected.Length, sut.Length);

            var actual = sut.ReadString(expected.Length, encoding);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Skip_one_and_ReadString_from_two_segments()
        {
            string expected = "The quick brown fox jumps over the lazy dog";
            Encoding encoding = Encoding.ASCII;

            List<ArraySegment<byte>> segments = GetSegments(expected, encoding, 2);

            BufferListReader sut = new BufferListReader();
            sut.Initialize(segments);

            Assert.AreEqual(expected.Length, sut.Length);

            var actual = sut.Skip(1).ReadString(expected.Length, encoding);

            Assert.AreEqual(expected.Substring(1), actual);
        }

        /// <summary>
        /// Split the string value into <paramref name="count"/> pieces.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<ArraySegment<byte>> GetSegments(string value, Encoding encoding, int count)
        {
            byte[] bytes;
            ArraySegment<byte> segment;
            List<ArraySegment<byte>> segments = new List<ArraySegment<byte>>();

            int startIndex = 0;

            for (int i = 0; i < count; i++)
            {
                int length = value.Length/count;
                if (i == (count - 1))
                {
                    length = value.Length - startIndex;
                }

                bytes = encoding.GetBytes(value.Substring(startIndex, length));
                startIndex += length;
                segment = new ArraySegment<byte>(bytes);
                segments.Add(segment);               
            }

            return segments;
        }
    }
}
