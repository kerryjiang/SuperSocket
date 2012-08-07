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

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (int i = 0; i < 100; i++)
            {
                source.Skip(offset).Take(length).ToArray();
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
    }
}
