using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "Protocol.FixedHeader")]
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

            protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
            {
                var strLen = buffer.GetString(Utf8Encoding);
                return int.Parse(strLen.TrimStart('0'));
            }

            protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.Slice(4).GetString(Utf8Encoding) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override IServer CreateServer(IHostConfigurator hostConfigurator)
        {
            return CreateSocketServerBuilder<TextPackageInfo, MyFixedHeaderPipelineFilter>(hostConfigurator)                
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                })
                .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:values:enableSendingPipe", "true" }
                    });
                }).BuildAsServer() as IServer;
        }
    }
}
