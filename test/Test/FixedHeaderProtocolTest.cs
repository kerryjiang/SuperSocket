using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Protocol.FixedHeader")]
    public class FixedHeaderProtocolTest : ProtocolTestBase
    {
        public FixedHeaderProtocolTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {

        }

        class MyFixedHeaderPipelineFilter : FixedHeaderPipelineFilter<TextPackageInfo>
        {
            public MyFixedHeaderPipelineFilter()
                : base(4)
            {

            }

            protected override int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer)
            {
                var strLen = buffer.GetString(Encoding.ASCII);
                return int.Parse(strLen.TrimStart('0'));
            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.Slice(4).GetString(Encoding.UTF8) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override SuperSocketServer CreateServer()
        {
            return CreateSocketServer<TextPackageInfo, MyFixedHeaderPipelineFilter>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            });
        }
    }
}
