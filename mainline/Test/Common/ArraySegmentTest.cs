using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;

namespace SuperSocket.Test.Common
{
    [TestFixture]
    public class ArraySegmentTest
    {
        [Test]
        public void TestCount()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegmentItem<char>>
                {
                    new ArraySegmentItem<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegmentItem<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.AreEqual(9, source.Count);
        }

        [Test]
        public void TestOutOfIndex()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegmentItem<char>>
                {
                    new ArraySegmentItem<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegmentItem<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[-1];
            });

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[10];
            });
        }

        [Test]
        public void TestIndexAccess()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegmentItem<char>>
                {
                    new ArraySegmentItem<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegmentItem<char>("Hello world!".ToCharArray(), 0, 4)
                });

            char[] exptected = "I lovHell".ToCharArray();

            for (int i = 0; i < source.Count; i++)
            {
                Assert.AreEqual(exptected[i], source[i]);
            }
        }

        [Test]
        public void TestIndexOf()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegmentItem<char>>
                {
                    new ArraySegmentItem<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegmentItem<char>("Hello world!".ToCharArray(), 0, 4)
                });

            //string exptected = "I lovHell";
            Assert.AreEqual(0, source.IndexOf('I'));
            Assert.AreEqual(2, source.IndexOf('l'));
            Assert.AreEqual(3, source.IndexOf('o'));
            Assert.AreEqual(4, source.IndexOf('v'));
        }

        [Test]
        public void TestEndsWith()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegmentItem<char>>
                {
                    new ArraySegmentItem<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegmentItem<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.AreEqual(true, source.EndsWith("Hell".ToCharArray()));
        }
    }
}
