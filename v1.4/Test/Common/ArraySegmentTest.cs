using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;
using System.Diagnostics;

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
            
            char currentChar = ' ';

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                currentChar = source[-1];
            });

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                currentChar = source[10];
            });

            source.RemoveSegmentAt(0);
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                currentChar = source[4];
            });

            source.RemoveSegmentAt(0);
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                currentChar = source[0];
            });

            source.AddSegment(new ArraySegment<char>("I love you,".ToCharArray(), 0, 5));
            source.AddSegment(new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4));

            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                currentChar = source[10];
            });
            
            Console.Write(currentChar);
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

            for (int i = source.Count - 1; i >= 0; i--)
            {
                Assert.AreEqual(exptected[i], source[i]);
            }

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
        public void TestIndexAccess2()
        {
            ArraySegmentList<char> sourceA = new ArraySegmentList<char>();
            List<char> sourceB = new List<char>();

            char[] element = null;

            for (var i = 0; i < 100; i++)
            {
                element = Guid.NewGuid().ToString().ToCharArray();
                sourceA.AddSegment(new ArraySegment<char>(element));
                sourceB.AddRange(element);
            }

            Random rd = new Random();

            for (int i = 0; i < 1000; i++)
            {
                int index = rd.Next(0, sourceA.Count - 1);
                Assert.AreEqual(sourceB[index], sourceA[index]);
            }

            int testCount = 10000;

            GC.Collect();

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (var i = 0; i < testCount; i++)
            {
                int index = rd.Next(0, sourceA.Count - 1);
                var tt = sourceA[index];
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();

            GC.Collect();

            watch.Start();

            for (var i = 0; i < testCount; i++)
            {
                int index = rd.Next(0, sourceA.Count - 1);
                var tt = sourceB[index];
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Test]
        public void TestIndexOf()
        {
            ArraySegmentList<char> source = new ArraySegmentList<char>(new List<ArraySegment<char>>
                {
                    new ArraySegment<char>("I love you,".ToCharArray(), 3, 7),
                    new ArraySegment<char>("Hello world!".ToCharArray(), 0, 4)
                });

            //string exptected = "ove youHell";
            Assert.AreEqual(-1, source.IndexOf('I'));
            Assert.AreEqual(9, source.IndexOf('l'));
            Assert.AreEqual(0, source.IndexOf('o'));
            Assert.AreEqual(1, source.IndexOf('v'));
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
            Assert.AreEqual("He", new string(source.ToArrayData(5, 2)));
            Assert.AreEqual("ovHe", new string(source.ToArrayData(3, 4)));
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

        [Test]
        public void TestSearchPerformance()
        {
            ArraySegmentList<char> sourceA = new ArraySegmentList<char>();
            List<char> sourceB = new List<char>();

            char[] element = null;

            for (var i = 0; i < 100; i++)
            {
                element = Guid.NewGuid().ToString().ToCharArray();
                sourceA.AddSegment(new ArraySegment<char>(element));
                sourceB.AddRange(element);
            }

            char[] mark = element.Take(4).ToArray();

            int testCount = 1000;

            GC.Collect();

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (var i = 0; i < testCount; i++)
            {
                sourceA.SearchMark(mark);
            }

            watch.Stop();
            
            Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();

            GC.Collect();

            watch.Start();

            for (var i = 0; i < testCount; i++)
            {
                sourceB.SearchMark(mark);
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}
