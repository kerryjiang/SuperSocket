using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;

namespace SuperSocket.Test.Common
{
    [TestFixture]
    public class BinaryUtilTest
    {
        [Test]
        public void SearchMarkNotFound()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so much!");
            byte[] mark = Encoding.ASCII.GetBytes("XY");

            Assert.IsFalse(BinaryUtil.SearchMark(source, mark).HasValue);
        }

        [Test]
        public void SearchMarkFake()
        {
            byte[] bytes = { 0x17, 0x17, 0x17, 0x17, 0x18 };
            byte[] mark = { 0x17, 0x17, 0x17, 0x18 };

            var actual = BinaryUtil.SearchMark(bytes, mark);

            var expected = 1;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SearchMarkBegin()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so much!");
            byte[] mark = Encoding.ASCII.GetBytes("I love");

            Assert.AreEqual(0, BinaryUtil.SearchMark(source, mark).Value);
        }

        [Test]
        public void SearchMarkMiddle()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so much!");
            byte[] mark = Encoding.ASCII.GetBytes("you");
            byte[] start = Encoding.ASCII.GetBytes("I love ");

            Assert.AreEqual(start.Length, BinaryUtil.SearchMark(source, mark).Value);
        }

        [Test]
        public void SearchMarkEnd()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so much!");
            byte[] mark = Encoding.ASCII.GetBytes("much!");
            byte[] start = Encoding.ASCII.GetBytes("I love you so ");

            Assert.AreEqual(start.Length, BinaryUtil.SearchMark(source, mark).Value);
        }

        [Test]
        public void SearchMarkMatchCount()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so mu");
            byte[] mark = Encoding.ASCII.GetBytes("much!");

            Assert.AreEqual(-2, BinaryUtil.SearchMark(source, mark).Value);
        }

        [Test]
        public void StartWithNotFound()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so mu");
            byte[] mark = Encoding.ASCII.GetBytes("xxx");

            Assert.AreEqual(-1, BinaryUtil.StartsWith(source, mark));
        }

        [Test]
        public void StartWithLenth()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so mu");
            byte[] mark = Encoding.ASCII.GetBytes("I love");

            Assert.AreEqual(mark.Length, BinaryUtil.StartsWith(source, mark));
        }

        [Test]
        public void StartWithLenth2()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love");
            byte[] mark = Encoding.ASCII.GetBytes("I love you so much");

            Assert.AreEqual(source.Length, BinaryUtil.StartsWith(source, mark));
        }

        [Test]
        public void TestEndsWith()
        {
            byte[] source = Encoding.ASCII.GetBytes("I love you so much");
            byte[] markA = Encoding.ASCII.GetBytes("much");
            byte[] markB = Encoding.ASCII.GetBytes("much1");
            Assert.AreEqual(true, source.EndsWith(markA));
            Assert.AreEqual(false, source.EndsWith(markB));
        }

        /// <summary>
        /// Tests the encoder.
        /// </summary>
        [Test]
        public void TestEncoder()
        {
            var decoder = Encoding.UTF8.GetDecoder();

            string message = "江振宇";

            byte[] source = Encoding.UTF8.GetBytes(message);

            Assert.AreEqual(9, source.Length);

            char[] dest = new char[3];

            byte[] partA = new byte[2];

            Array.Copy(source, 0, partA, 0, 2);

            byte[] partB = new byte[source.Length - 2];

            Array.Copy(source, 2, partB, 0, partB.Length);

            int bytesUsed, charsUsed;
            bool completed;

            int totalChars = 0;

            decoder.Convert(partA, 0, partA.Length, dest, 0, dest.Length, false, out bytesUsed, out charsUsed, out completed);
            Console.WriteLine(charsUsed);
            totalChars += charsUsed;
            decoder.Convert(partB, 0, partB.Length, dest, charsUsed, dest.Length - charsUsed, true, out bytesUsed, out charsUsed, out completed);
            Console.WriteLine(charsUsed);
            totalChars += charsUsed;

            Assert.AreEqual(message, new string(dest, 0, totalChars));

        }
    }
}
