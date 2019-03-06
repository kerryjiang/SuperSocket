using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Protocol.FixedSize")]
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

        protected override SuperSocketServer CreateServer()
        {
            return CreateSocketServer<TextPackageInfo, MyFixedSizePipelineFilter>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            });
        }
    }
}
