using System;
using System.Buffers;
using System.Text;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Trait("Category", "Protocol.FixedSize")]
    public class FixedSizeProtocolTest : ProtocolTestBase
    {
        public FixedSizeProtocolTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {

        }

        class MyFixedSizePipelineFilter : FixedSizePipelineFilter<TextPackageInfo>
        {
            public MyFixedSizePipelineFilter()
                : base(36)
            {

            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine;
        }

        protected override IServer CreateServer()
        {
            var server = CreateSocketServerBuilder<TextPackageInfo, MyFixedSizePipelineFilter>()
                .ConfigurePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer() as IServer;
                
            return server;
        }
    }
}
