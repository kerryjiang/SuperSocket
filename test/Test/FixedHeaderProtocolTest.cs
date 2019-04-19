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
                var strLen = buffer.GetString(Utf8Encoding);
                return int.Parse(strLen.TrimStart('0'));
            }

            protected override TextPackageInfo DecodePackage(ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.Slice(4).GetString(Utf8Encoding) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override IServer CreateServer()
        {
            return CreateSocketServerBuilder<TextPackageInfo, MyFixedHeaderPipelineFilter>()
                .ConfigurePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Utf8Encoding.GetBytes(p.Text + "\r\n")));
                }).BuildAsServer() as IServer;
        }
    }
}
