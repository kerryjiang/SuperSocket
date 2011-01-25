using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace SuperSocket.Test.Common
{
    [TestFixture]
    public class PerformanceTest
    {
        //[Test]
        public void TestCopyArrayByLINQ()
        {
            byte[] source = new byte[1024 * 1024 * 10]; //10MB

            int offset = 1024 * 1024 * 5;
            int length = 1024 * 1024 * 5;

            byte[] target;

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (int i = 0; i < 100; i++)
            {
                target = source.Skip(offset).Take(length).ToArray();
                Console.WriteLine(i);
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Test]
        public void TestCopyArray()
        {
            byte[] source = new byte[1024 * 1024 * 10]; //10MB

            int offset = 1024 * 1024 * 5;
            int length = 1024 * 1024 * 5;

            byte[] target = new byte[length];

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (int i = 0; i < 100; i++)
            {
                Array.Copy(source, offset, target, 0, length);
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Test]
        public void TestCopyArrayByBlockCopy()
        {
            byte[] source = new byte[1024 * 1024 * 10]; //10MB

            int offset = 1024 * 1024 * 5;
            int length = 1024 * 1024 * 5;

            byte[] target = new byte[length];

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (int i = 0; i < 100; i++)
            {
                Buffer.BlockCopy(source, offset, target, 0, length);
            }

            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);
        }

        [Test]
        [TestCase(50000, 10)]
        [TestCase(500000, 10)]
        [TestCase(5000000, 10)]
        public void CompareIntDictionaryAndArray(int testCount, int testRepeat)
        {
            Stopwatch watch = new Stopwatch();

            //Prepare data
            Dictionary<int, int> dict = new Dictionary<int, int>(testCount);
            int[] array = new int[testCount];

            for (int i = 0; i < testCount; i++)
            {
                dict.Add(i, i);
                array[i] = i;
            }

            watch.Start();

            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < testCount; i++)
                {
                    var sample = array[i];
                }
            }

            watch.Stop();

            long arraySpan = watch.ElapsedTicks;

            Console.WriteLine(arraySpan);

            watch.Reset();
            watch.Start();

            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < testCount; i++)
                {
                    var sample = array[i];
                    var result = dict[sample];
                }
            }

            watch.Stop();

            long dictSpan = watch.ElapsedTicks;

            Console.WriteLine(dictSpan);

            GC.Collect();
        }

        [Test]
        [TestCase(50000, 10)]
        [TestCase(500000, 10)]
        [TestCase(5000000, 10)]
        public void CompareStringDictionaryAndArray(int testCount, int testRepeat)
        {
            Stopwatch watch = new Stopwatch();

            //Prepare data
            Dictionary<string, string> dict = new Dictionary<string, string>(testCount);
            string[] array = new string[testCount];

            for (int i = 0; i < testCount; i++)
            {
                var sample = Guid.NewGuid().ToString();
                dict.Add(sample, sample);
                array[i] = sample;
            }

            watch.Start();

            for (int j = 0; j < testRepeat; j++)
            {
                for (int i = 0; i < testCount; i++)
                {
                    var sample = array[i];
                }
            }

            watch.Stop();

            long arraySpan = watch.ElapsedMilliseconds;

            Console.WriteLine(arraySpan);

            watch.Reset();
            watch.Start();

            for (int j = 0; j < testRepeat; j++)
            {
                for (int i = 0; i < testCount; i++)
                {
                    var sample = array[i];
                    var result = dict[sample];
                }
            }

            watch.Stop();

            long dictSpan = watch.ElapsedMilliseconds;

            Console.WriteLine(dictSpan);
            Console.WriteLine(watch.ElapsedTicks / (testCount * testRepeat));

            GC.Collect();
        }
    }
}
