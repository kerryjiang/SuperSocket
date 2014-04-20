using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.ProtoBase;

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
    }
}
