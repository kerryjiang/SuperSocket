using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "Protocol.Terminator")]
    public class TerminatorProtocolTest : ProtocolTestBase
    {
        public TerminatorProtocolTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {

        }

        protected override string CreateRequest(string sourceLine)
        {
            return $"{sourceLine}##";
        }

        protected override IServer CreateServer(IHostConfigurator hostConfigurator)
        {
            var server = CreateSocketServerBuilder<TextPackageInfo>((x) => new TerminatorTextPipelineFilter(new[] { (byte)'#', (byte)'#' }), hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer() as IServer;

            return server;
        }
    }
}
