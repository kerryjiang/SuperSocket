using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.Common;
using System.Threading;

namespace SuperSocket.Test.Common
{
    [TestFixture]
    public class BatchQueueTest
    {
        [Test, Repeat(100)]
        public void TestFastEnqueue()
        {
            int count = 1000;

            var source = new string[count * 3];
            var output = new string[count * 3];

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = Guid.NewGuid().ToString();
            }

            IBatchQueue<string> queue = new ConcurrentBatchQueue<string>(output);

            Parallel.For(0, count, (i) =>
                {
                    var index = (int)i;
                    var sw = new SpinWait();

                    queue.Enqueue(source[index * 3]);

                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();

                    queue.Enqueue(source[index * 3 + 1]);

                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();

                    queue.Enqueue(source[index * 3 + 2]);
                });

            for (var i = 0; i < queue.Count; i++)
            {
                if (null == output[i])
                    Assert.Fail();
            }
        }

        [Test, Repeat(100)]
        public void TestFastBatchEnqueue()
        {
            int count = 1000;

            var source = new string[count * 3];
            var output = new string[count * 3];

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = Guid.NewGuid().ToString();
            }

            IBatchQueue<string> queue = new ConcurrentBatchQueue<string>(output);

            Parallel.For(0, count, (i) =>
            {
                var index = (int)i;
                queue.Enqueue(new string[] { source[index * 3], source[index * 3 + 1], source[index * 3 + 2] });
            });

            for (var i = 0; i < queue.Count; i++)
            {
                if (null == output[i])
                    Assert.Fail();
            }
        }


        [Test, Repeat(100)]
        public void TestFastEnqueueDequeue()
        {
            int count = 1000;

            var source = new string[count * 3];

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = Guid.NewGuid().ToString();
            }

            IBatchQueue<string> queue = new ConcurrentBatchQueue<string>(count * 3);

            Task[] tasks = new Task[count];

            for (var i = 0; i < count; i++)
            {
                tasks[i] = new Task((j) =>
                {
                    var index = (int)j;
                    var sw = new SpinWait();

                    queue.Enqueue(source[index * 3]);

                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();

                    queue.Enqueue(source[index * 3 + 1]);

                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();
                    sw.SpinOnce();

                    queue.Enqueue(source[index * 3 + 2]);
                }, i);
            }

            tasks.ToList().ForEach(t => t.Start());

            var outputList = new List<string>();

            var spinWait = new SpinWait();

            while (outputList.Count < source.Length)
            {
                if (!queue.TryDequeue(outputList))
                    spinWait.SpinOnce();

                if (spinWait.Count >= 1000)
                    Assert.Fail("Failed to dequeue all items on time!");
            }

            var dict = source.ToDictionary(s => s);

            for (var i = 0; i < outputList.Count; i++)
            {
                var line = outputList[i];

                if (!dict.Remove(line))
                    Assert.Fail("Dequeue error");
            }

            Assert.AreEqual(0, dict.Count);
        }

        [Test, Repeat(100)]
        public void TestFastBatchEnqueueDequeue()
        {
            int count = 1000;

            var source = new string[count * 3];

            for (var i = 0; i < source.Length; i++)
            {
                source[i] = Guid.NewGuid().ToString();
            }

            IBatchQueue<string> queue = new ConcurrentBatchQueue<string>(count * 3);

            Task[] tasks = new Task[count];

            for (var i = 0; i < count; i++)
            {
                tasks[i] = new Task((j) =>
                {
                    var index = (int)j;
                    queue.Enqueue(new string[] { source[index * 3], source[index * 3 + 1], source[index * 3 + 2] });
                }, i);
            }

            tasks.ToList().ForEach(t => t.Start());

            var outputList = new List<string>();

            var spinWait = new SpinWait();

            while (outputList.Count < source.Length)
            {
                if (!queue.TryDequeue(outputList))
                    spinWait.SpinOnce();
                else
                    Console.WriteLine("Count:" + outputList.Count);

                if (spinWait.Count >= 1000)
                    Assert.Fail("Failed to dequeue all items on time!");
            }

            var dict = source.ToDictionary(s => s);

            for (var i = 0; i < outputList.Count; i++)
            {
                var line = outputList[i];

                if (!dict.Remove(line))
                    Assert.Fail("Dequeue error");
            }

            Assert.AreEqual(0, dict.Count);
        }
    }
}
