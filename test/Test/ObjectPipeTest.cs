using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using System.Threading;
using SuperSocket.Channel;

namespace Tests
{
    [Collection("ObjectPipe")]
    public class ObjectPipeTest : TestClassBase
    {
        public ObjectPipeTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        private IObjectPipe<T> CreatePipe<T>()
        {
            return new DefaultObjectPipe<T>();
        }
        

        [Fact] 
        public async Task TestOneWriteOneRead()
        {
            var pipe = CreatePipe<int>();

            for (var i = 0; i < 100; i++)
            {
                pipe.Write(i);

                var result = await pipe.ReadAsync();

                Assert.Equal(i, result);
            }
        }

        [Fact] 
        public async Task TestMultipleWriteReadLater()
        {
            var pipe = CreatePipe<int>();

            for (var i = 0; i < 100; i++)
            {
                pipe.Write(i);
            }

            for (var i = 0; i < 100; i++)
            {
                var result = await pipe.ReadAsync();
                Assert.Equal(i, result);
            }
        }

        [Fact] 
        public async Task TesConcurrentWriteRead()
        {
            var pipe = CreatePipe<int>();

            var t = Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    pipe.Write(i);
                }
            });

            for (var i = 0; i < 100; i++)
            {
                var result = await pipe.ReadAsync();
                Assert.Equal(i, result);
            }

            t.Wait();
        }
    }
}
