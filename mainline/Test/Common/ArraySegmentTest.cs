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
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.AreEqual(9, source.Count);

            source.RemoveSegmentAt(0);
            Assert.AreEqual(4, source.Count);

            source.RemoveSegmentAt(0);
            Assert.AreEqual(0, source.Count);

            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));

            Assert.AreEqual(9, source.Count);
        }

        [Test]
        public void TestOutOfIndex()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[-1];
            });

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[10];
            });

            source.RemoveSegmentAt(0);
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[4];
            });

            source.RemoveSegmentAt(0);
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[0];
            });

            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                char currentChar = source[10];
            });
        }

        [Test]
        public void TestIndexAccess()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            char[] exptected = "I lovHell".ToCharArray();

            for (int i = 0; i < source.Count; i++)
            {
                Assert.AreEqual(exptected[i], source[i]);
            }

            source.RemoveSegmentAt(0);
            source.RemoveSegmentAt(0);           

            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));

            for (int i = 0; i < source.Count; i++)
            {
                Assert.AreEqual(exptected[i], source[i], i + " is expected!");
            }
        }

        [Test]
        public void TestIndexOf()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
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
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            Assert.AreEqual(true, source.EndsWith("Hell".ToCharArray()));
        }

        [Test]
        public void TestToArray()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 0, 5),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            char[] exptected = "I lovHell".ToCharArray();

            Assert.AreEqual(exptected, source.ToArrayData());
        }

        [Test]
        public void TestRemoveSegment()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>();
            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));
            Assert.AreEqual(9, source.Count);
            Assert.AreEqual(2, source.SegmentCount);

            source.RemoveSegmentAt(1);
            Assert.AreEqual(5, source.Count);
            Assert.AreEqual(1, source.SegmentCount);

            char[] exptected = "I lov".ToCharArray();

            for (var i = 0; i < exptected.Length; i++)
            {
                Assert.AreEqual(exptected[i], source[i]);
            }
        }

        [Test]
        public void TestAddSegment()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>();

            Assert.AreEqual(0, source.Count);

            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            Assert.AreEqual(5, source.Count);
            Assert.AreEqual(1, source.SegmentCount);
            
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));
            Assert.AreEqual(9, source.Count);
            Assert.AreEqual(2, source.SegmentCount);

            char[] exptected = "I lovHell".ToCharArray();

            for (var i = 0; i < exptected.Length; i++)
            {
                Assert.AreEqual(exptected[i], source[i]);
            }
        }
    }
}
